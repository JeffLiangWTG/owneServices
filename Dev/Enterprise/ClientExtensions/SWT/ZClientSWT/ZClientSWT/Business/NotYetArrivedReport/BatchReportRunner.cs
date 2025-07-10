using System.Collections.Generic;
using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.SWT
{
	class BatchReportRunner
	{
		public void RunBatchReports(INotifications notifications, CancellationToken token)
		{
			FactoryProvider.CreateNewWithoutSave();
			DataProvider.LoadData(FactoryProvider.Current);

			if (DataProvider.HasData)
			{
				RunReportWithDataCore(token);
			}
			else
			{
				notifications.Notify(new InfoNotification("No data has been found to run the 'Not Yet Arrived' report"));
			}
		}

		internal void InternalRunBatchReportsTest(INotifications notifications) => RunBatchReports(notifications, CancellationToken.None);

		protected virtual void RunReportWithDataCore(CancellationToken token)
		{
			foreach (ZGuid consigneePK in DataProvider.ConsigneePKs)
			{
				token.ThrowIfCancellationRequested();
				OrgHeader consignee = FactoryProvider.Current.Load<OrgHeader>(consigneePK);
				if (consignee != null)
				{
					RunReportForEachOrganisation(consignee, "Importer", ConsigneeCodeCollection);
				}
			}

			foreach (ZGuid consignorPK in DataProvider.ConsignorPKs)
			{
				token.ThrowIfCancellationRequested();
				OrgHeader consignor = FactoryProvider.Current.Load<OrgHeader>(consignorPK);
				if (consignor != null)
				{
					RunReportForEachOrganisation(consignor, "Supplier", ConsignorCodeCollection);
				}
			}
		}

		void RunReportForEachOrganisation(OrgHeader org, ZString filterType, IList<ZString> contactTypeCodeCollection)
		{
			using (ReportPrintSet printset = new ReportPrintSet(NotYetArrivedReportCommand))
			{
				DocumentPack docPack = printset[0];
				Report report = docPack.GetFirstReport();

				DeliveryInstructions instructions = new DeliveryInstructions(docPack);

				instructions.Destination = DeliveryInstructionDestination.TakenFromContact;
				SetInstructions(instructions, org, contactTypeCodeCollection);

				if (instructions.Recipients.Count > 0)
				{
					report.PrepareForRender();
					ReportFiltersSetter reportFilter = new ReportFiltersSetter(filterType, org);
					reportFilter.UpdateReportFilters(report.FilterCollection);

					PrintTask task = new PrintTask();
					task.Add(docPack);
					task.Run(instructions);
				}
			}
		}

		protected virtual void SetInstructions(DeliveryInstructions instructions, OrgHeader consignee, IList<ZString> contactTypeCode)
		{
			instructions.Recipients.RemoveAll();
			foreach (OrgContact contact in consignee.Contacts)
			{
				OrgDocument doc = GetDocContactForAllOrDocType(contact, contactTypeCode);
				if (doc != null && HasAllSettingsCorrect(doc, contact))
				{
					DocDeliveryContact docContact = instructions.Recipients.AddNew();
					docContact.DeliveryMethod = doc.OD_DeliverBy;
					docContact.AttachmentType = doc.OD_AttachmentType;
					docContact.OrgHeaderPK = consignee.PK;
					docContact.Fax = contact.OC_Fax;
					docContact.Email = contact.OC_Email;
					docContact.Name = contact.OC_ContactName;
				}
			}
		}

		protected OrgDocument GetDocContactForAllOrDocType(OrgContact contact, IList<ZString> contactTypeCode)
		{
			return GetDocGroupByGroupType(contact, contactTypeCode) ?? GetDocGroupByGroupType(contact, AnyContactCodeCollection);
		}

		OrgDocument GetDocGroupByGroupType(OrgContact contact, IList<ZString> groupType)
		{
			OrgDocument result = null;
			foreach (OrgDocument doc in contact.Documents)
			{
				if (groupType.Contains(doc.OD_DocumentGroup))
				{
					result = doc;
					break;
				}
			}
			return result;
		}

		bool HasAllSettingsCorrect(OrgDocument doc, OrgContact contact)
		{
			return (doc.OD_DeliverBy == Core.Constants.ContactNotifyModes.Fax && !contact.OC_Fax.IsEmpty)
			|| (doc.OD_DeliverBy == Core.Constants.ContactNotifyModes.Email && !contact.OC_Email.IsEmpty);
		}

		#region NotYetArrivedReportCommand

		protected ReportCommand NotYetArrivedReportCommand
		{
			get { return notYetArrivedReportCommand ?? (notYetArrivedReportCommand = GetReportCommand()); }
		}
		ReportCommand notYetArrivedReportCommand;

		protected virtual ReportCommand GetReportCommand()
		{
			ZDBOnlyQuery menuItemQuery = new ZDBOnlyQuery(typeof(ReportCommand));
			ZDBOnlySubQuery pivotSubQuery = new ZDBOnlySubQuery(typeof(StmMenuTemplatePivotBase), StmMenuTemplatePivotSchema.SI_SU);
			pivotSubQuery.AddToFilter(StmMenuTemplatePivotSchema.SI_SO, NotYetArrivedTemplate.PK);
			menuItemQuery.AddSubQuery(pivotSubQuery, JoinCondition.And);

			return FactoryProvider.Current.LoadTop1<ReportCommand>(menuItemQuery);
		}

		#endregion

		#region NotYetArrivedTemplate

		StmTemplate NotYetArrivedTemplate
		{
			get
			{
				if (fTemplate == null)
				{
					ZQuery filter = new ZQuery(StmTemplateSchema.SO_Name, "SWT Not Yet Arrived Report");
					filter.AddToFilter(StmTemplateSchema.SO_IsClientSpecific, "Y");
					filter.AddToFilter(StmTemplateSchema.SO_DataContext, "NONE");
					filter.AddToFilter(StmTemplateSchema.SO_IsSystemDefined, "Y");
					fTemplate = FactoryProvider.Current.LoadTop1<StmTemplateBase>(filter);
				}
				return fTemplate;
			}
		}
		StmTemplate fTemplate;

		#endregion

		#region ReportDataProvider

		ReportDataProvider DataProvider
		{
			get { return dataProvider ?? (dataProvider = new ReportDataProvider()); }
		}
		ReportDataProvider dataProvider;

		#endregion

		#region FactoryProvider

		protected BusinessObjectFactoryProvider FactoryProvider
		{
			get { return factoryProvider ?? (factoryProvider = new BusinessObjectFactoryProvider()); }
		}
		BusinessObjectFactoryProvider factoryProvider;

		#endregion

		IList<ZString> ConsignorCodeCollection
		{
			get
			{
				if (consignorCodeCollection == null)
				{
					consignorCodeCollection = new List<ZString>(1);
					consignorCodeCollection.Add(ContactType.Consignor.Code);
				}
				return consignorCodeCollection;
			}
		}
		List<ZString> consignorCodeCollection;

		IList<ZString> ConsigneeCodeCollection
		{
			get
			{
				if (consigneeCodeCollection == null)
				{
					consigneeCodeCollection = new List<ZString>(2);
					consigneeCodeCollection.Add(ContactType.Consignee.Code);
					consigneeCodeCollection.Add(ContactType.Warehouse3PL.Code);
				}
				return consigneeCodeCollection;
			}
		}
		List<ZString> consigneeCodeCollection;

		IList<ZString> AnyContactCodeCollection
		{
			get
			{
				if (anyContactCodeCollection == null)
				{
					anyContactCodeCollection = new List<ZString>(1);
					anyContactCodeCollection.Add(ContactType.All.Code);
				}
				return anyContactCodeCollection;
			}
		}
		IList<ZString> anyContactCodeCollection;
	}
}

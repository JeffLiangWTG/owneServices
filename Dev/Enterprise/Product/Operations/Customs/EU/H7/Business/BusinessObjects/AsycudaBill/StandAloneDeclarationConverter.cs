using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;

namespace Enterprise.Customs.EU.H7.Business
{
	public class StandAloneDeclarationConverter
	{
		public void TryConvert(IEnumerable<AsycudaBill> selectedBills)
		{
			var isMutipleBills = selectedBills.Count() > 1;
			foreach (var bill in selectedBills)
			{
				if (TryConvert(bill.PK, isMutipleBills))
				{
					bill.Reload();
					bill.LockBillIfConverted();
				}
			}
		}

		bool TryConvert(ZGuid pk, bool isMutipleBills)
		{
			var factory = new UniversalObjectFactory(new BusinessObjectFactory());
			var bill = factory.Load<AsycudaBill>(pk);
			var dataObject = WriteFromBill(bill);
			var logger = ObjectFactory.Get<IXmlSessionTracker>("IXmlSessionTracker", new SimpleLogger());
			var declaration = ReadToDeclaration(dataObject, factory, logger);
			try
			{
				if (declaration != null)
				{
					factory.BOFactory.Saving += factory_Saving;

					if (isMutipleBills)
					{
						factory.SaveAtEndOfImport(logger);
					}

					OnCompleted?.Invoke(declaration, new EventArgs());
					return true;
				}
				else
				{
					OnError.Invoke(logger.ToString(), new EventArgs());
				}
			}
			catch (Exception e)
			{
				OnError.Invoke(e.Message, new EventArgs());
			}
			finally
			{
				factory.BOFactory.Saving -= factory_Saving;
			}

			return false;

			void factory_Saving(BusinessObjectFactory e) => OnSaving(bill, declaration);
		}

		Shipment WriteFromBill(AsycudaBill bill)
		{
			var writer = bill.Header.ApplicationBusinessProvider.GetCustomsDeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BRO, bill)), bill.Header);
			var dataObject = writer.GetDataObject(bill);
			dataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			return dataObject;
		}

		JobDeclaration ReadToDeclaration(Shipment shipment, UniversalObjectFactory factory, IXmlSessionTracker logger)
		{
			var declarationContextManager = DataContextType.CustomsDeclaration.GetUniversalDataContextManager() as IShipmentDataContextManagerInternal;
			var reader = declarationContextManager.GetShipmentDataObjectReader(shipment, logger, factory);
			return reader.ReadIntoTopLevelBusinessObject() as JobDeclaration;
		}

		void OnSaving(AsycudaBill bill, JobDeclaration declaration)
		{
			declaration.PopulateJE_DeclarationReferenceIfNeeded();
			bill.ABL_IsActive = false;
			bill.EntrySummaryReferenceNumber = declaration.JE_DeclarationReference.Left(Common.AutoCusEntryNum.Schema.CE_EntryLineReferenceMaxLength);
		}

		public event EventHandler OnCompleted;
		public event EventHandler OnError;
	}
}

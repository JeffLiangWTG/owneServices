using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class SupportEdwDataSourceReport : RegistryBusinessObjectTemplate
	{
		#region Schema

		class Schema
		{
			public const string ReportName = "ReportName";
			public const string BusinessContext = "BusinessContext";
		}

		#endregion

		public SupportEdwDataSourceReport()
			: base()
		{
		}

		public SupportEdwDataSourceReport(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new SupportEdwDataSourceReport(fallbackLevel, factory);
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateReportName();
			ValidateBusinessContext();
		}

		#region Properties

		public ZString ReportName
		{
			get { return fReportName; }
			set
			{
				SetNonPersistentPropertyValue<ZString>(ReportNameInfo, ref fReportName, value);
				if (!IsValidationSuspended)
				{
					ValidateReportName();
				}
			}
		}
		ZString fReportName;

		public ZPropertyInfo ReportNameInfo
		{
			get { return GetZPropertyInfo(Schema.ReportName); }
		}

		public ZString BusinessContext
		{
			get { return fBusinessContext; }
			set
			{
				SetNonPersistentPropertyValue<ZString>(BusinessContextInfo, ref fBusinessContext, value);
				if (!IsValidationSuspended)
				{
					ValidateBusinessContext();
				}
			}
		}
		ZString fBusinessContext;

		public ZPropertyInfo BusinessContextInfo
		{
			get { return GetZPropertyInfo(Schema.BusinessContext); }
		}

		#endregion

		#region Validation

		public void ValidateReportName()
		{
			ReportNameInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ReportNameInfo);

			if (!ReportNameInfo.HasErrors())
			{
				var errorMessage = CheckUniqueAndValid();
				if (!errorMessage.IsEmpty)
				{
					ReportNameInfo.AddError(errorMessage);
				}
			}
		}

		public void ValidateBusinessContext()
		{
			BusinessContextInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(BusinessContextInfo);

			if (!BusinessContextInfo.HasErrors())
			{
				var errorMessage = CheckUniqueAndValid();
				if (!errorMessage.IsEmpty)
				{
					BusinessContextInfo.AddError(errorMessage);
				}
			}
		}

		ZString CheckUniqueAndValid()
		{
			var result = ZString.Empty;
			if (!ReportName.IsEmpty && !BusinessContext.IsEmpty)
			{
				if (ParentCollection != null && ParentCollection.Cast<SupportEdwDataSourceReport>().Any(x => x.PK != PK && x.ReportName == ReportName && x.BusinessContext == BusinessContext))
				{
					return Res.GetString("2f9d5ca9-40bb-4b17-9c37-3c83ef0037a5", "Report Name and Business Context must be unique.");
				}

				var query = new ZQuery();
				query.AddToFilter(StmMenuItemSchema.SU_MenuName, ReportName);
				query.AddToFilter(StmMenuItemSchema.SU_BusinessContext, BusinessContext);
				var stmMenuItems = CurrentFactory.Load<IStmMenuItem>(query);
				if (stmMenuItems.Any())
				{
					if (stmMenuItems.Any(x => x.SU_MenuName != ReportName || x.SU_BusinessContext != BusinessContext))
					{
						return Res.GetString("b1a4f1a1-12e2-4e80-b318-f460f2b1e7db", "Please enter the correct name: Report Name:'{0}' Business Context:'{1}'", stmMenuItems[0].SU_MenuName, stmMenuItems[0].SU_BusinessContext);
					}
				}
				else
				{
					return Res.GetString("6aa76a9c-d725-4dbf-9b51-a4d34ea29816", "Report Name and Business Context don't match any report.");
				}
			}

			return result;
		}

		SupportEdwDataSourceReportCollection ParentCollection
		{
			get { return (SupportEdwDataSourceReportCollection)GetParentCollection(this, typeof(SupportEdwDataSourceReportCollection)); }
		}

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.ReportName, ReportName);
			writer.WriteElementString(Schema.BusinessContext, BusinessContext);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			ReportName = new ZString(reader.ReadElementString(Schema.ReportName));
			BusinessContext = new ZString(reader.ReadElementString(Schema.BusinessContext));
		}

		#endregion
	}
}

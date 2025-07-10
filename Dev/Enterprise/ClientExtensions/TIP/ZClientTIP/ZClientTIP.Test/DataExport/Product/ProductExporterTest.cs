using System;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Client.TIP.Testing
{
	public class ProductExporterTest : TestCaseWithFactory
	{
		[TestDate(2007, 2, 16, 15, 10, 10)]
		public void TestExport()
		{
			Part.Factory.Save();
			OrgPartRelationRegistryBusinessObjectCollection orgCollection = new OrgPartRelationRegistryBusinessObjectCollection();
			OrgPartRelationRegistryBusinessObject org = (OrgPartRelationRegistryBusinessObject)orgCollection.AddNew();
			org.OrgHeaderPK = Owner.PK;
			org.RelationshipType = OrgPartRelation.RelationshipTypes.Owner;
			TIPDataRegistry.Instance.OrganisationProductRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, orgCollection);
			string expectedFileName = Path.Combine(Env.TempPath, owner.OH_Code + "_OWN_20070216151010.csv");
			try
			{
				Exporter.Export();
				Assert("File should have been created", File.Exists(expectedFileName));
				AssertContains("Product shortlisted info", $"Product '{Part.OP_PartNum}': [Organisation ='{org.Organisation.OH_Code}', Relationship ='{org.RelationshipType}'] is shortlisted for export.", Buffer.AsString);
				AssertContains("Product data record creation", $"Generating Product Records for Organisation ='{org.Organisation.OH_Code}' Relationship ='{org.RelationshipType}'.", Buffer.AsString);
				AssertContains("Product count and file name", "Estimated # of Products: 1. \r\nOutput File Name:", Buffer.AsString);
			}
			finally
			{
				DeleteIfExists(expectedFileName);
			}

			AssertNotNull(Part.Logs.MostRecentLogByEventTime(Events.DataExport, TIPConstants.ProductDEXReference));
		}

		[TestDate(2007, 2, 16, 15, 10, 10)]
		public void TestFileDeletedWhenEmpty()
		{
			Part.Factory.Save();
			string expectedFileName = Path.Combine(Env.TempPath, owner.OH_Code + "_OWN_20070216151010.csv");
			OrgPartRelationRegistryBusinessObjectCollection orgCollection = new OrgPartRelationRegistryBusinessObjectCollection();
			OrgPartRelationRegistryBusinessObject org = (OrgPartRelationRegistryBusinessObject)orgCollection.AddNew();
			org.OrgHeaderPK = owner.PK;
			org.RelationshipType = OrgPartRelation.RelationshipTypes.Supplier;
			TIPDataRegistry.Instance.OrganisationProductRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, orgCollection);
			try
			{
				Exporter.Export();
				AssertNotContains("Product shortlisted info", $"Product '{Part.OP_PartNum}': [Organisation ='{org.Organisation.OH_Code}', Relationship ='{org.RelationshipType}'] is shortlisted for export.", Buffer.AsString);
				AssertNotContains("Product data record creation", $"Generating Product Records for Organisation ='{org.Organisation.OH_Code}' Relationship ='{org.RelationshipType}'.", Buffer.AsString);
				AssertNotContains("Product count and file name", "Estimated # of Products: 1. \r\nOutput File Name:", Buffer.AsString);
				Assert("File should not exist", !File.Exists(expectedFileName));
			}
			finally
			{
				DeleteIfExists(expectedFileName);
			}
		}

		#region Implementation
		OrgHeader Owner
		{
			get
			{
				return owner ?? (owner = Factory.LoadTop1<OrgHeader>(new ZQuery()));
			}
		}

		OrgHeader owner;
		AUOrgSupplierPart Part
		{
			get
			{
				if (part == null)
				{
					part = Factory.New<AUOrgSupplierPart>();
					part.OP_PartNum = "TESTPART";
					OrgPartRelation orgPart = part.RelatedOrganisations.AddNew();
					orgPart.OU_OH = Owner.PK;
					orgPart.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
					AUCClass importTariff = Factory.New<AUCClass>();
					importTariff.UJ_Code = "2203.00.31 15";
					importTariff.UJ_UQ1 = "LA";
					importTariff.UJ_UQ2 = "L";
					Classification importClass = part.ClassificationsForBinding.AddNew();
					importClass.CC_ClassificationType = Classification.ClassificationType.IMP;
					importClass.CC_TariffNum = importTariff.UJ_Code;
					importClass.CC_LookupCode = "I9999988888";
					importClass.AddInfo.ZA_ORG = Core.Constants.CountryCodes.Germany;
					importClass.AddInfo.ZA_PRF = "E";
					importClass.AddInfo.ZA_TreatmentCode_Hidden = "DTR";
					importClass.AddInfo.ZA_InstrumentType_Hidden = "DN";
					importClass.AddInfo.ZA_InstrumentCode_Hidden = "111111";
					importClass.AddInfo.ZA_DTY = 18.0m;
					part.PivotsForBinding[0].CI_ChildType = Customs.Business.ClassificationTypeList.Codes.HTI;
				}

				return part;
			}
		}

		AUOrgSupplierPart part;
		protected override void SetUp()
		{
			base.SetUp();
			Buffer.Clear();
			DataTransferRegistryBusinessObject newValue = new DataTransferRegistryBusinessObject(Factory);
			newValue.Directory = Env.TempPath;
			newValue.UpdateRuns(ZDateTime.Now);
			GlbGroup postmasterGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			newValue.GroupPK = postmasterGroup.PK;
			TIPDataRegistry.Instance.ProductExportRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue);
		}

		ProductExporter Exporter
		{
			get
			{
				return exporter ?? (exporter = new ProductExporter(Buffer));
			}
		}

		ProductExporter exporter;
		NotificationBuffer Buffer
		{
			get
			{
				return buffer ?? (buffer = new NotificationBuffer());
			}
		}

		NotificationBuffer buffer;
		#endregion
	}
}

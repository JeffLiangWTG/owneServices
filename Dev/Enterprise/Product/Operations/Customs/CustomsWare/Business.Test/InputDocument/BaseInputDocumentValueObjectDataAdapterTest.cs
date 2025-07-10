using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.CustomsWare.Business.Testing
{
	[TestedType(typeof(InputDocumentValueObjectDataAdapter))]
	public class BaseInputDocumentValueObjectDataAdapterTest : ValueObjectDataAdapterTest<BaseJobDeclaration, XSD.InputDocument>
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Consumed tests have SOURCE_CODE")]
		protected void DoImport(ZString fileName, ZString countryCode)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				var dataAdapter = InputDocumentValueObjectDataAdapter.New();
				var decCollection = new BaseJobDeclarationCollection(Factory);
				using (var stream = new FileStream(testPath + fileName, FileMode.Open, FileAccess.Read))
				{
					var serializer = new InputDocumentXMLValueObjectSerializer();
					serializer.ImportXmlData(stream, dataAdapter, decCollection, new SingleBusinessObjectFactoryProvider(Factory), new NotificationBuffer());
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Baseline")]
		protected string testPath = BaseSourcePath + @"Enterprise\Product\Operations\Customs\CustomsWare\Business.Test\Processors\TestData\";
		#region Implementation
		protected BaseJobDeclaration Declaration
		{
			get
			{
				if (dec == null)
				{
					dec = Factory.New<BaseJobDeclaration>();
					dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
					dec.JE_HouseBill = "HOUSE";
					dec.JE_MasterBill = "MASTER";
					var supplier = Factory.New<OrgHeader>();
					supplier.OH_FullName = "Supplier Full Name";
					supplier.MainAddress.OA_Address1 = "Supplier Address1";
					supplier.MainAddress.OA_Address2 = "Supplier Address2";
					supplier.MainAddress.OA_City = "Los Angeles";
					supplier.MainAddress.OA_PostCode = "8485";
					supplier.MainAddress.OA_RL_NKRelatedPortCode = "USLAX";
					dec.JE_OH_Supplier = supplier.PK;
					var importer = Factory.New<OrgHeader>();
					importer.OH_FullName = "Importer Full Name";
					importer.MainAddress.OA_Address1 = "Importer Address1";
					importer.MainAddress.OA_Address2 = "Importer Address2";
					importer.MainAddress.OA_City = "London";
					importer.MainAddress.OA_PostCode = "P3434";
					importer.MainAddress.OA_RL_NKRelatedPortCode = "GBLON";
					dec.JE_OH_Importer = importer.PK;
					dec.JE_GoodsDescription = "Goods Description";
					dec.JE_TotalNoOfPacks = 326;
					dec.JE_TotalWeight = 765.2354;
					dec.JE_TotalWeightUnit = Core.Constants.Weight.Tonnes;
					dec.JE_ShipmentIncoTerm = Core.Constants.IncoTerms.CostAndFreight;
					dec.JE_TransportMode = Core.Constants.TransportModes.Sea;
					dec.JE_GoodsDescription = "GOODS DESCRIPTION";
					dec.JE_GB = GlbBranch.CurrentBranch.PK;
					dec.JE_RL_NKPortOfLoading = "USLAX";
					dec.JE_RL_NKFinalDestination = "GBLON";
					dec.JE_RL_NKPortOfArrival = "FRXYZ";
					dec.JE_GoodsDestination = "";
					var container = dec.CusContainers.AddNew();
					container.CO_ContainerNumber = "CONTAINER1";
					container.CO_Seal = "SEAL";
					container.CO_ContainerSize = "20";
					var containerType = Factory.New<RefContainer>();
					containerType.RC_Code = "20XJ";
					container.CO_RC = containerType.PK;
					dec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				}

				return dec;
			}
		}

		BaseJobDeclaration dec;
		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Switzerland);
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
			branchInOtherCountry = Factory.NewWithValidTestData<GlbBranch>();
			branchInOtherCountry.GB_GC = company.PK;
		}

		protected GlbBranch branchInOtherCountry;
		protected override ValueObjectDataAdapter<BaseJobDeclaration, XSD.InputDocument> GetNewBizObjXmlDataAdapter()
		{
			return InputDocumentValueObjectDataAdapter.New();
		}

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects()
		{
			return null;
		}

		protected override string ExpectedRootCollectionElementName
		{
			get
			{
				return "InputDocuments";
			}
		}

		protected override string ExpectedRootElementName
		{
			get
			{
				return "InputDocument";
			}
		}

		protected override bool IsExportToCollectionSupported
		{
			get
			{
				return false;
			}
		}

		protected override bool IsExportToValueObjectSupported
		{
			get
			{
				return true;
			}
		}

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			return null;
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			return null;
		}

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample()
		{
			return null;
		}

		protected override void TestExportToAndImportFromAndExportToValueObject(BusinessObjectAndExpectedOutputFileName sample)
		{
			Assert("Not Relevant because round tripping not required in this instance", true);
		}

		public override void TestTestCoverageOfValueObject()
		{
			Assert("Not Relevant this is an Xsd supplied by a third party", true);
		}

		protected override void TestExportToValueObject(BusinessObjectAndExpectedOutputFileName sample)
		{
			Assert("Specific cases that are required to work will be tested", true);
		}

		protected override string[] StringFieldsToIgnore
		{
			get
			{
				return new string[] { "AttachmentStream", "DocumentRef", "DocumentSubRef" };
			}
		}
		#endregion
	}
}

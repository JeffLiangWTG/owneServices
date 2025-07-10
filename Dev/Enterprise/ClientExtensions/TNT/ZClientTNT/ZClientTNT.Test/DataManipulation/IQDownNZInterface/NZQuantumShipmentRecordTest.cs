using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.TNT.NZ.Testing
{
	public class NZQuantumShipmentRecordTest : TestCaseWithFactory
	{
		[TestDate(2006, 1, 13)]
		public void TestCreateNewDeclarationForShipment()
		{
			NZQuantumShipmentRecord shipmentRecord = new NZQuantumShipmentRecord(GlbBranch.CurrentBranch.GB_Code, "", TestType03RecordNoEntryNum);
			ForwardingShipment shipment = shipmentRecord.CreateShipment(Factory, true, new NotificationBuffer());
			Factory.Save();
			AssertEquals("One new Shipment created", 1, Factory.GetDatabaseCount(typeof(CommonShipment)));
			AssertEquals("No Declaration should be created", 0, Factory.GetDatabaseCount(typeof(JobDeclaration)));
			Assert("Shipment Customs Entry Number should be empty", shipment.CustomsEntryNumber.IsEmpty);
			AssertEquals("Package Type on the Shipment should be set to Package", Constants.PkgUnit.Package, shipment.JS_F3_NKPackType);
		}

		public void TestFindFirstMatchingShipmentCore()
		{
			NZQuantumShipmentRecord shipmentRecord = new NZQuantumShipmentRecord(GlbBranch.CurrentBranch.GB_Code, "", TestType03RecordNoEntryNum);
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>(TestBusinessObjectKind.MinimumRequiredToSave);
			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			shipment1.JS_HouseBill = "HBL90Q8UE";
			AssertNull("No shipment should be found", shipmentRecord.FindFirstMatchingShipment(consol));
			ForwardingShipment shipment2 = consol.Shipments.AddNew();
			shipment2.JS_HouseBill = shipmentRecord.HouseBill;
			AssertEquals("Shipment with Housebill " + shipmentRecord.HouseBill + " should be found", shipment2.PK, shipmentRecord.FindFirstMatchingShipment(consol).PK);
			ForwardingConsol consol2 = Factory.NewWithValidTestData<ForwardingConsol>(TestBusinessObjectKind.MinimumRequiredToSave);
			ForwardingShipment shipment3 = consol2.Shipments.AddNew();
			shipment3.JS_HouseBill = "A9FN9W8EYFD";
			AssertNull("Should search for shipments attached to the current consol, not all shipments in database", shipmentRecord.FindFirstMatchingShipment(consol2));
		}

		public void TestGetConsignorByLegacyCode()
		{
			NZQuantumShipmentRecordForTest shipmentRecord = new NZQuantumShipmentRecordForTest(GlbBranch.CurrentBranch.GB_Code, "", TestType03RecordNoEntryNum);
			TemporaryOrganisationCreator creator = new TemporaryOrganisationCreator(Factory);
			NotificationBuffer buffer = new NotificationBuffer();
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>(TestBusinessObjectKind.MinimumRequiredToSave);
			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			shipment1.JS_HouseBill = shipmentRecord.HouseBill;
			OrgHeader consignorWithMapping = Factory.LoadTop1<OrgHeader>(new ZQuery());
			SetupOrganisationWithCodeMapping(consignorWithMapping, shipmentRecord.ConsignorCountry + shipmentRecord.ConsignorLegacyCode, Core.Constants.CountryCodes.Australia);
			AssertNull("Couldn't find a match for the organisation", shipmentRecord.GetConsignorByLegacyCode(Factory, buffer, creator));
			SetupOrganisationWithCodeMapping(consignorWithMapping, shipmentRecord.ConsignorCountry + shipmentRecord.ConsignorLegacyCode, Core.Constants.CountryCodes.NewZealand);
			OrgHeader consignorReturned = shipmentRecord.GetConsignorByLegacyCode(Factory, buffer, creator);
			AssertNotNull("organisation returned", consignorReturned);
			AssertEquals("organisation returned", consignorWithMapping, consignorReturned);
		}

		void SetupOrganisationWithCodeMapping(OrgHeader org, ZString legacyCode, ZString countryCode)
		{
			OrgCusCode cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.LegacySystemCode;
			cusCode.OK_RN_NKCodeCountry = countryCode;
			cusCode.OK_CustomsRegNo = legacyCode;
			Factory.Save();
		}

		class NZQuantumShipmentRecordForTest : NZQuantumShipmentRecord
		{
			public NZQuantumShipmentRecordForTest(ZString branchCode, ZString mBagNo, ZString line) : base(branchCode, mBagNo, line)
			{
			}

			public new OrgHeader GetConsignorByLegacyCode(BusinessObjectFactory factory, INotifications notify, TemporaryOrganisationCreator temporaryCreator)
			{
				return base.GetConsignorByLegacyCode(factory, notify, temporaryCreator);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.NewZealand);
			TestCaseHelper.ClearTable(JobShipmentSchema.Constants.TableName);
			TestCaseHelper.ClearTable(JobDeclarationSchema.Constants.TableName);
		}

		const string TestType03RecordNoEntryNum = "03930941449 SYDJXO20525452SYDNEY BOUTIQUE WINE           11B REDAN ST                                                  MOSMAN                         NEW SOUTH WALES                AU 2088     0299693601  0299693601                                                                                                                                                                                                                                             VILLAGE CELLARS                6-5 UENO , UWADA               HIMI                           HIMI                           TOYAMA                         JP 9350056  0766728680  0766728680  YOSHIKO NAKAMURA                                                                                                                                                                                                                           NS    292115.08JPY     1   522.000ECN                                                          T                         .";
	}
}

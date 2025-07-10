using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.ManifestBase;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaManifestHeader))]
	sealed class AsycudaManifestHeaderBaseOnlyTest : AsycudaManifestHeaderAbstractTest
	{
		public void TestBills()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertType<ASYCUDA.Business.AsycudaBillCollection<AsycudaBill, AsycudaManifestHeader>>(header.Bills);
		}

		public void TestIsICSManifest()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ManifestType = "XXX";
			AssertEquals("Not ICS manifest", false, header.IsICSManifest);
			header.AMA_ManifestType = EUManifestTypes.Codes.ICS;
			AssertEquals("ICS manifest", true, header.IsICSManifest);
		}

		public void TestIsTransportModes()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals(false, header.IsInlandTransport);
			header.AMA_TransportMode = Core.Constants.TransportModes.InlandWaterwayTransport;
			AssertEquals(true, header.IsInlandTransport);
		}

		public void TestDelete()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();
			var euOfficeCode = header.EUCustomsOffices.AddNew();
			header.Delete();
			Assert(euOfficeCode.IsDeleted);
		}

		public void TestCustomsOfficeCodes()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();
			AssertEquals("CustomsOffices", typeof(IcsOfficeCodeCollection), header.EUCustomsOffices.GetType());
		}

		public void TestGetIcsOfficeCodeValidation()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();
			var officeCode = header.EUCustomsOffices.AddNew();
			AssertType<IcsOfficeCodeValidation>(header.GetIcsOfficeCodeValidation(officeCode));
		}
	}

	public abstract class AsycudaManifestHeaderAbstractTest : ASYCUDA.Business.Testing.AsycudaManifestHeaderAbstractTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<AsycudaManifestHeader>();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.SuspendCheckBusinessObjectType();
			return header;
		}

		protected override Type ExpectedTypeOfContainer => typeof(AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader>);
	}
}

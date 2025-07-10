using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.Module.Testing
{
	[TestedType(typeof(EUH7Controller))]
	sealed class EUH7ControllerTest : ZControllerBasherTest
	{
		public override void TestNewForm()
		{
			Assert("Form not implemented yet", true);
		}

		public override void TestViewForm()
		{
			Assert("Form not implemented yet", true);
		}

		public override void TestEditForm()
		{
			Assert("Form not implemented yet", true);
		}

		public override void TestDeleteForm()
		{
			Assert("Form not implemented yet", true);
		}

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.EU.EUH7;

		public void TestGetNewBusinessEntityInLocalFactory()
		{
			var controller = new EUH7ControllerForTest();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Ireland))
			{
				var ieHeader = (AsycudaManifestHeader)controller.GetNewBusinessEntityInLocalFactoryExposed();
				AssertEquals("LV1", ieHeader.AMA_ApplicationCode);
				AssertEquals(ObjectFactory.GetType<Integration.Customs.IEH7.IAsycudaManifestHeader>(), ieHeader.GetType());
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Finland))
			{
				var euHeader = (AsycudaManifestHeader)controller.GetNewBusinessEntityInLocalFactoryExposed();
				AssertEquals("LVC", euHeader.AMA_ApplicationCode);
				AssertEquals(ObjectFactory.GetType<Integration.Customs.ASYCUDA.EUH7.IAsycudaManifestHeader>(), euHeader.GetType());
			}
		}

		class EUH7ControllerForTest : EUH7Controller
		{
			internal IBusiness GetNewBusinessEntityInLocalFactoryExposed()
			{
				return base.GetNewBusinessEntityInLocalFactory();
			}
		}
	}
}

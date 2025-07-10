using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ES.Manifest.H7.Business;
using Enterprise.Customs.EU.H7.GUI.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Manifest.H7.Module.Testing
{
	[TestedType(typeof(G3DeclarationFilterControl))]
	sealed class G3DeclarationFilterControlTest : TestCaseWithFactory
	{
		public void TestComponents()
		{
			var g3Declaration = Factory.New<G3EDIMessage>();
			g3Declaration.EM_MessageType = "G3D";

			var collection = new G3DeclarationMessageCollection(Factory);
			collection.Load();

			var filterObj = new G3DeclarationFilterBusinessObject();

			using (var control = new G3DeclarationFilterControl(collection, filterObj))
			{
				control.Show();

				var grid = control.FindSingle<ZFilterGrid>();

				EUH7GUITestHelper.AssertGridLayout(grid,
					[
						"Header+AMA_JobReference",
						"Header+AMA_TransportMode",
						"Header+AMA_RL_NKPortOfDischarge",
						"Header+AMA_E_ARV",
						"Header+AMA_RN_NKConveyanceNationality",
						"Header+AMA_VesselName",
						"Header+AMA_Voyage",
						"Header+AMA_RN_NKCountry",
						"Header+AMA_MessageStatus",
						"Header+AMA_Nature",
						"Header+AMA_E_DEP",
						"Header+AMA_RL_NKPortOfLoading",
						"Header+AMA_AgentType",
						"Header+AMA_ManifestNumber",
						"Header+RegistrationDate",
						"Header+PresentationOffice",
						"Header+Declarant+Header+OH_Code",
						"Header+Presenter+Header+OH_Code",
						"BillsCount",
						"Header+AMA_MasterInformation",
						"EM_MessageNum",
						"EM_ReceiveTransmit",
						"EM_MessageType",
						"EM_Status",
						"EM_MessageDateTime",
						"EM_DateTimeInterchangeSent",
						"EM_InterchangeNumber",
						"EM_InterchangeStatus"
					]);
			}
		}
	}
}

using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Business.Testing
{
	[TestedType(typeof(DCAParametersRegistryItem))]
	sealed class DCAParametersRegistryItemTest : StronglyTypedRegistryItemTestCase<DCAParameters, DCAParameters>
	{
		public void TestDefaultValue()
		{
			var currentCompany = GlbCompany.CurrentCompany;
			var defaultDCAParameter = ILCustomsDataRegistry.Instance.DCAParametersForASyncMessage.GetValueWithoutFallback(currentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			AssertEquals(5, defaultDCAParameter.Services.Count);
			AssertContainsExactElementsInAnyOrder(new ZString[]
			{
				"SendMN_MSG1220_DeliveryOrderFeedBack_Message",
				"SendGP_MSG1035_GatepassFeedbackMessage",
				"SendMN_MSG1171_SendManifestFeedBack_Message",
				"SendVAL_MSG8227_RequiredDocumentMessage",
				"SendVAL_MSG8228_RequiredDocumentVerificationDecisionMessage",
			}, defaultDCAParameter.Services.Cast<DCAService>().Select(x => x.Name));
		}

		protected override StronglyTypedRegistryItem<DCAParameters, DCAParameters> GetNewRegistryItem()
		{
			return new DCAParametersRegistryItem("", (NoResString)"", (NoResString)"", (NoResString)"", RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport);
		}
	}
}

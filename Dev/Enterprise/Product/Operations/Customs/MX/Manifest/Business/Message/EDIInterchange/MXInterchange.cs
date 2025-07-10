using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.MX.Manifest.Business
{
	public class MXInterchange : EDIInterchange
	{
		public MXInterchange(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			EI_ApplicationCode = ApplicationCodes.MXCustoms;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override Type GetMessageTypeToCreate(ZString messageText) => typeof(MXMessage);

		protected override bool ShouldSendViaEHubCore => EI_TransportType != EDIInterchange.TransportType.xT;
	}
}

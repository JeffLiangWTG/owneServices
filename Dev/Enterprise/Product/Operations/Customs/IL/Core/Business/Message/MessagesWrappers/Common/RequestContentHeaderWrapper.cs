using System;
using System.Collections.ObjectModel;
using CargoWise.Customs.IL.MessageDefinitions.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IL.Business
{
	public sealed class RequestContentHeaderWrapper : IRequestContentHeader
	{
		public static IRequestContentHeader New()
			=> new RequestContentHeaderWrapper();

		public string Convertor => null;

		public Collection<int> RecieverId => new Collection<int> { ZInt.Zero };

		public int SenderId
			=> int.TryParse(GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(GlbCompany.CurrentCompany)?.GlbExternalPassword.GP_MailBoxID, out var companyVAT)
			? companyVAT
			: 0;

		public DateTime TransmitionDateTime => ZDateTime.UtcNow.ToDateTime();
	}
}

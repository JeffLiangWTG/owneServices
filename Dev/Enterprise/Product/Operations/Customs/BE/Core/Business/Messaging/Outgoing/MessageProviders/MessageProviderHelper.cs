using System;
using System.Collections.Generic;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BE.Business;

public static class MessageProviderHelper
{
	public static string ReturnNullIfEmpty<T>(T input) where T : IZType => input.IsEmpty ? null : input.ToString();

	public static int? IntReturnNullIfEmpty(ZString input) => !input.IsEmpty && int.TryParse(input, out int result) ? result : null;

	public static decimal? ReturnNullIfEmpty(ZDecimal input) => input.IsEmpty ? null : (decimal?)input;

	public static decimal? DecimalReturnNullIfEmpty(ZString input) => !input.IsEmpty && decimal.TryParse(input, out var result) ? result : null;

	public static DateTime? ReturnNullIfEmpty(ZDateTime input) => input.IsEmpty ? null : input.ToDateTime();

	public static DateTime GetCurrentDateTimeAsUnspecifiedDateTimeKind(bool removeMillisecond = true)
	{
		return DateTimeProviderHelper.ConvertToUnspecifiedDateTimeKindIfPossible(ZDateTime.Now, removeMillisecond);
	}

	public static IReadOnlyCollection<ICommunication> GetCommunicationsFromOrgHeader(OrgHeader orgHeader)
	{
		var result = new List<ICommunication>();
		if (orgHeader?.AllocatedContacts.GetAllocatedContact(OrgConstants.ContactAllocationType.CUS) is OrgContact contact)
		{
			var email = contact.OC_Email;
			var phone = contact.OC_Phone;
			if (!email.IsEmpty)
			{
				result.Add(new PNTSCommunicationProvider(email, CommunicationType.Codes.EM));
			}
			if (!phone.IsEmpty)
			{
				result.Add(new PNTSCommunicationProvider(phone, CommunicationType.Codes.TE));
			}
		}
		return result;
	}
}

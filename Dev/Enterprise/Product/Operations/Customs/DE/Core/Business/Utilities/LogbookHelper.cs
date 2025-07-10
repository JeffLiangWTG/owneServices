using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.DE.Business
{
	[SystemDefinedValues]
	public static class LogbookHelper
	{
		public static void SetLogbookEORIBranchSuffix(this EDIMessage message, ZString value)
		{
			if (message != null && !value.IsEmpty)
			{
				message.SetSystemDefinedValue("LogbookEORIBranchSuffix", value);
			}
		}

		public static void SetLogbookRegistrationNumber(this EDIMessage message, ZString value) => message.SetLogbookRegistrationNumber(new ZString[] { value });

		public static void SetLogbookRegistrationNumber(this EDIMessage message, IEnumerable<ZString> registrationNumbers)
			=> message.CreateOrUpdateNote(LogbookRegistrationNumberNoteDescription, registrationNumbers.Where(x => !x.IsEmpty).Distinct().ToArray(), LogbookRegistrationNumberNoteDescriptionSeperator);

		public static void SetLogbookLocalReferenceNumber(this EDIMessage message, ZString localReferenceNumber)
			=> message.CreateOrUpdateNote(LogbookLocalReferenceNumberNoteDescription, localReferenceNumber);

		public const string LogbookRegistrationNumberNoteDescription = "RegistrationNumber";

		public const string LogbookRegistrationNumberNoteDescriptionSeperator = ", ";

		public const string LogbookLocalReferenceNumberNoteDescription = "LocalReferenceNumber";

		public const string LogbookGUAMainAccessCode = "GUAMainAccessCode";
	}
}

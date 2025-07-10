
//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Business;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.OperationalAction
{
	public static class Constants
	{
		public static string NoDeclarationToSend
		{
			get { return Res.GetString("B64DDB31-3462-402D-9AD4-0DBA4A76102E", "No Declaration has been selected to send."); }
		}

		public static string NoDeclarationToMerge
		{
			get { return Res.GetString("962B2364-8453-45E0-9625-71356971FB16", "No Declaration has been selected to merge."); }
		}

		public static string TooManyDeclarationToMerge
		{
			get { return Res.GetString("F5E92956-234A-412D-9BAA-8AC50C0710BF", "Select too many declarations to merge. A maximum of 50 declarations can be merged at one time."); }
		}

		public static string Successful
		{
			get { return Res.GetString("9914edbb-7c04-44f1-9cef-7be09e4a6838", "[Successful]"); }
		}

		public static string Failed
		{
			get { return Res.GetString("00a1873e-10c7-4ca9-b5c0-4b7c17e5c883", "[Failed]"); }
		}

		public static string Seperator
		{
			get { return "----------------------------------------"; }
		}

		public static string DeclarationNotEligable
		{
			get { return Res.GetString("748515f3-6679-49e4-8b6f-01d0d9a1d426", "The Declaration is not eligible to send Entry Message since it's not a Import Job or LVS Job."); }
		}

		public static string DeclarationSubmitThroughInterface
		{
			get { return Res.GetString("4ca073cd-bd02-4a9d-9183-53f459d91016", "The Declaration is not eligible to send Entry message because this job is configured to submit through a designated service provider interface."); }
		}

		public static string DeclarationNotEligableForRNSQuery
		{
			get { return Res.GetString("E54380DD-2BC3-44A3-A84D-BCE0848756DD", "The Declaration is not eligible to send RNS Query since it's not a Import Job."); }
		}

		public static string InvalidDeclaration
		{
			get { return Res.GetString("6c804287-fdd3-4994-9259-e4258ddbe1a0", "The Declaration is invalid."); }
		}

		public static string EncounterError
		{
			get { return Res.GetString("2be0cd02-483c-4011-a714-858128607c6f", "[Encounter Error when processing]"); }
		}

		public static string AwaitingConfirmation
		{
			get { return Res.GetString("81ef51c7-f22c-43d1-9800-ad72b7e01846", "[Awaiting User's Confirmation]:"); }
		}

		public static string AwaitingAnswer
		{
			get { return Res.GetString("91057c7c-edb8-4195-a3e6-0bf11662c3e5", "[Awaiting User's Answer]:"); }
		}

		public static string UsersAnswer
		{
			get { return Res.GetString("0862fd6a-bc89-4a13-9c17-91640c1f66ba", "[User's Answer]:"); }
		}

		public static string AutoAnswer
		{
			get { return Res.GetString("2DF80DE2-A093-4B89-A1A6-67A91AB3C266", "[Automatic Answer]:"); }
		}

		public static string ProcessingDeclaration
		{
			get { return Res.GetString("74C1418D-E1B7-4967-ADBA-303B79FEA7C9", "Processing"); }
		}

		public static string SendingMessageForDeclaration
		{
			get { return Res.GetString("F5C5E662-ECCE-4E48-AAA8-D4C6BF434363", "Sending message for"); }
		}

		public static string SuccessfullyMergedDeclaration
		{
			get { return Res.GetString("377CC0FD-9BF3-4012-BBB5-4FEBA342AD9E", "Successfully merge entry for"); }
		}

		public static string CannotMergeDeclaration
		{
			get { return Res.GetString("35C416F5-9348-4B87-A2A6-DC6E0CFDEB74", "Can't merge"); }
		}

		public static string MergeFailedReason
		{
			get { return Res.GetString("45FE6D8E-1A71-48F1-8A93-83D51F9472CD", ". Merge failed reason : "); }
		}

		public static string NotifyFormatParaHolder
		{
			get { return " {0}"; }
		}

		public static string ContinueSendingDespiteOfRationalityWarnings
		{
			get { return Res.GetString("3676FF48-84ED-4E4B-93B6-019485A8CFC7", "Continue sending despite of rationality warnings."); }
		}

		public static string MessageSendingCancelled
		{
			get { return Res.GetString("4E8B910D-EE0E-4D0F-8AB3-A31EA309BE37", "Message sending canceled."); }
		}
	}
}

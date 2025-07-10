namespace Enterprise.Customs.IT.Business;

static class ITPreviousDocumentValidationMessages
{
	public static class NmrnReferenceNumberValidationMessages
	{
		public static string NoSpacesAreAllowed => Res.GetString(
			"906732C0-B1E5-4970-A69C-D0B5D93B532D",
			"This type of document must contain an MRN or a registration number as per the following example: 5T-137893-2020-279100 (Procedure, Number without CIN, Year, Customs Office). No spaces are allowed.");

		public static string MrnOrRegistration => Res.GetString(
			"B0E84D89-9871-439E-A78C-FFA1D0C5DA51",
			"This type of document must contain an MRN (18 characters) or a registration number as per the following example: 5T-137893-2020-279100 (Procedure, Number without CIN, Year, Customs Office).");

		public static string EnterAllRequiredFields => Res.GetString(
			"4235CAE-23D0-4A59-9CBA-090C934EF46E",
			"{0}. You didn't enter all the fields required.",
			NR0047RuleExampleMessage);

		public static string MoreFieldsThanRequired => Res.GetString(
			"F858A7AA-3A09-4C1D-A751-E37CF716F81C",
			"{0}. You entered more fields than required.",
			NR0047RuleExampleMessage);

		public static string ProcedureIsEmpty => Res.GetString(
			"F39F1AB9-BD06-44AD-9160-140D632FA1E2",
			"{0}, the first part must contain a Procedure.",
			NR0047RuleExampleMessage);

		public static string RegistrationIsNotNumeric => Res.GetString(
			"B9FC9A25-10AD-4C0F-9092-521D66F4B700",
			"{0}, the second part must contain a number of registration",
			NR0047RuleExampleMessage);

		public static string RegistrationIsMoreThan8Digits => Res.GetString(
			"2A9986A6-E936-487A-AC07-9551B457CED6",
			"{0}, the second part must contain a number of registration between 1 and 8 digits.",
			NR0047RuleExampleMessage);

		public static string YearOfIssuing => Res.GetString(
			"FA2A3D63-BA25-48DE-8725-956691207F29",
			"{0}, the third part must contain the year of issuing.",
			NR0047RuleExampleMessage);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		const string NR0047RuleExampleMessage = "Reference fields must be as for the following example: 5T-137893-2020-279100 (Procedure, Number without CIN, Year, Customs Office)";
	}
}


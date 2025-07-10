
using CargoWise.Common;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DE.Business.Declaration
{
	sealed class DeclarationEmailSubjectSuffixProvider
	{
		static string LrnCaption => Res.GetString("EA424952-F1B9-4A85-AE26-1DB36E98190E", "- LRN:");

		readonly string suffix;

		public DeclarationEmailSubjectSuffixProvider(CusEntryHeader entryHeader)
		{
			Argument.NotNull(entryHeader, nameof(entryHeader));
			suffix = $"{LrnCaption} {entryHeader.LocalReferenceNumber}";
		}

		public void SetEmailSubjectSuffix(EmailDef email)
		{
			email.Subject = $"{email.Subject} {suffix}";
		}
	}
}

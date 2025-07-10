using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.DocumentWrappers.Customs.EU
{
	public class ImporterBisPageTraderBox : IBisPageTraderBox
	{
		public ImporterBisPageTraderBox(DocSADH docSADH)
		{
			DocSADH = Argument.NotNull(docSADH, nameof(docSADH));
			Declaration = Argument.NotNull(DocSADH.Declaration, nameof(DocSADH.Declaration));
		}

		DocSADH DocSADH { get; }

		JobDeclaration Declaration { get; }

		ZString IBisPageTraderBox.Caption => DocSADH.Box8LabelCaption;

		ZString IBisPageTraderBox.ID => Declaration.ImporterTraderId;

		ZString IBisPageTraderBox.Content
		{
			get
			{
				var importer = Declaration.Importer;
				var fullName = importer?.OH_FullName ?? ZString.Empty;
				var country = DocSADH.ShowBox8ImporterCountryCode ? importer?.Country?.RN_Desc ?? ZString.Empty : ZString.Empty;
				return FormattableString.Invariant($"{fullName}{System.Environment.NewLine}{country}").Trim();
			}
		}
	}
}

using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.DocumentWrappers.Customs.EU
{
	public class SupplierBisPageTraderBox : IBisPageTraderBox
	{
		public SupplierBisPageTraderBox(DocSADH docSADH)
		{
			DocSADH = Argument.NotNull(docSADH, nameof(docSADH));
			Declaration = Argument.NotNull(DocSADH.Declaration, nameof(DocSADH.Declaration));
		}

		DocSADH DocSADH { get; }

		JobDeclaration Declaration { get; }

		ZString IBisPageTraderBox.Caption => DocSADH.Box2LabelCaption;

		ZString IBisPageTraderBox.ID => Declaration.SupplierTraderId;

		ZString IBisPageTraderBox.Content
		{
			get
			{
				var supplier = Declaration.Supplier;
				var fullName = supplier?.OH_FullName ?? ZString.Empty;
				var country = DocSADH.ShowBox2SupplierCountryCode ? supplier?.Country?.RN_Desc ?? ZString.Empty : ZString.Empty;
				return FormattableString.Invariant($"{fullName}{System.Environment.NewLine}{country}").Trim();
			}
		}
	}
}

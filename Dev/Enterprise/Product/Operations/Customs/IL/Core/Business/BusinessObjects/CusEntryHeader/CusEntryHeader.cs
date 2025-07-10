namespace Enterprise.Customs.IL.Business
{
	public partial class CusEntryHeader
	{
		public override void OnSaving()
		{
			base.OnSaving();

			PopulateCH_BGMReferenceIfNeeded();
		}

		const string ReferenceNumberSeparator = "/";

		void PopulateCH_BGMReferenceIfNeeded()
		{
			if (CH_BGMReference.IsEmpty)
			{
				var declaration = Declaration;
				if (declaration != null)
				{
					declaration.PopulateJE_DeclarationReferenceIfNeeded();

					var reference = declaration.JE_DeclarationReference + ReferenceNumberSeparator + CalculateHeaderID();

					CH_BGMReference = reference;
				}
			}
		}

		int CalculateHeaderID()
		{
			var result = -1;

			var declaration = Declaration;
			if (declaration != null)
			{
				var numberInDeclarationHeaderCollection = -1;
				var headerNumber = 0;
				foreach (var header in declaration.CustomsEntryHeaders)
				{
					headerNumber++;
					if (header == this)
					{
						numberInDeclarationHeaderCollection = headerNumber;
						break;
					}
				}

				result = numberInDeclarationHeaderCollection;
			}

			return result;
		}
	}
}

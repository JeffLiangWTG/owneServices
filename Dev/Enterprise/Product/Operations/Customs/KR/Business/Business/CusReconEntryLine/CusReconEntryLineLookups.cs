using CargoWise.EntityFramework;

namespace Enterprise.Customs.KR.Business
{
	public class CusReconEntryLineLookups(CusReconEntryLine parent) : Customs.Business.CusReconEntryLineLookups(parent)
	{
		public KREntryLineDetailsViewCollection ImportEntryLineNumbers
		{
			get
			{
				return Factory.GetCachedValue($"KREntryLineDetailsViewCollection.ImportEntryLineNumbers_{Parent.Header.CRE_OriginalEntryNumber}_{Parent.FormattedOriginalEntryLineNumber}", () =>
				{
					var result = new KREntryLineDetailsViewCollection(Factory, Parent.Header.CRE_OriginalEntryNumber);
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Entry Number", "Property", Parent.Header.CRE_OriginalEntryNumber, false));
					if (!Parent.FormattedOriginalEntryLineNumber.IsEmpty)
					{
						result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Entry Line No.", "Property", Parent.FormattedOriginalEntryLineNumber, true));
					}
					return result;
				});
			}
		}
		public new CusReconEntryLine Parent => (CusReconEntryLine)base.Parent;
	}
}

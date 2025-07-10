//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccTaxReturnLookups
//
//    This class should be used for overriding collections in AutoAccTaxReturnLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ComplianceReport
{
	public class AccTaxReturnLookups : AutoAccTaxReturnLookups
	{
		public AccTaxReturnLookups(AutoAccTaxReturn parent) : base(parent)
		{
		}

		protected new AccTaxReturn Parent => base.Parent as AccTaxReturn;

		public virtual CodeDescriptionPairList TaxReturnStatusList
		{
			get
			{
				var result = GetTaxReturnStatusList();
				if (Parent.ATR_ReturnType == AccTaxReturn.ReturnType.MTD)
				{
					result.RemoveCode(AccTaxReturn.Status.Generated);
				}
				return result;
			}
		}

		public static CodeDescriptionPairList GetTaxReturnStatusList()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(AccTaxReturn.Status.Saved, Res.GetString("4021d629-f22f-463b-a8bf-36b604ae3dff", "Saved"));
			result.AddPair(AccTaxReturn.Status.Generated, Res.GetString("779752af-0855-4ea2-8850-4a252e3dd729", "Generated"));
			result.AddPair(AccTaxReturn.Status.Submitted, Res.GetString("e8643167-4df8-448f-99c2-08e9dc7479eb", "Submitted"));
			return result;
		}
	}
}
using System;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AUStateCodeValidation : ZValidation
	{
		public AUStateCodeValidation(AUStateCode parent)
			: base(parent)
		{
			this.aUStateCodeBusinessObject = parent;
		}

		public override void ValidateAll()
		{
			ValidateCode();
		}

		public override Type AutoValidationType
		{
			get { return typeof(AUStateCode); }
		}

		#region Code

		public void ValidateCode()
		{
			ValidateCalculatedProperty(aUStateCodeBusinessObject.CodeInfo);
		}
		#endregion

		protected void CheckCode()
		{
			if (!aUStateCodeBusinessObject.IsValidationSuspended)
			{
				ListValidation.ErrorIfInvalidCode(aUStateCodeBusinessObject.CodeInfo, aUStateCodeBusinessObject.Lookups.AUStateCodeList);

				if (aUStateCodeBusinessObject.ParentCollections.Count > 0)
				{
					AUStateCodeCollection collection = ((AUStateCodeCollection)aUStateCodeBusinessObject.ParentCollections.First());

					if (collection.Cast<AUStateCode>().Count(x => x.Code == aUStateCodeBusinessObject.Code) > 1)
					{
						aUStateCodeBusinessObject.CodeInfo.AddError(string.Format(CultureInfo.InvariantCulture, "Duplicated AU State Codes on this invoice line, (AU State Code : {0})", aUStateCodeBusinessObject.Code));
					}
				}
			}
		}
		readonly AUStateCode aUStateCodeBusinessObject;
	}
}

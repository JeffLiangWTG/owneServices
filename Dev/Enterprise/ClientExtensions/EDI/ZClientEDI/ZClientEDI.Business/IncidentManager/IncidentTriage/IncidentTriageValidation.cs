//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoIncidentTriageValidation
//
//    This class should be used for overriding validation in AutoIncidentTriageValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;
using CargoWise.EntityFramework;
using Res = ZClientEDI.Business.Res;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class IncidentTriageValidation : AutoIncidentTriageValidation
	{
		public IncidentTriageValidation(AutoIncidentTriage parent) : base(parent)
		{
		}

		new IncidentTriage Parent
		{
			get { return (IncidentTriage)base.Parent; }
		}

		protected override void CheckIMT_Type()
		{
			base.CheckIMT_Type();
			MandatoryValidation.CheckEntered(Parent.IMT_TypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.IMT_TypeInfo);
		}

		protected override void CheckIMT_Level()
		{
			base.CheckIMT_Level();
			MandatoryValidation.CheckEntered(Parent.IMT_LevelInfo);
			ListValidation.ErrorIfInvalidCode(Parent.IMT_LevelInfo);
		}

		protected override void CheckIMT_SupportDescription()
		{
			base.CheckIMT_Type();
			MandatoryValidation.CheckEntered(Parent.IMT_SupportDescriptionInfo);
		}

		protected override void CheckIMT_Product()
		{
			base.CheckIMT_Product();
			ValidateProductDetails(Parent.IMT_ProductInfo);
		}

		protected override void CheckIMT_ProductArea()
		{
			base.CheckIMT_ProductArea();
			ValidateProductDetails(Parent.IMT_ProductAreaInfo);
		}

		protected override void CheckIMT_Module()
		{
			base.CheckIMT_Module();
			ValidateProductDetails(Parent.IMT_ModuleInfo);

			if (Parent.IMT_SetProductAreaByMenuItem)
			{
				MandatoryValidation.CheckEntered(Parent.IMT_ModuleInfo);
			}
		}

		void ValidateProductDetails(ZPropertyInfo propertyInfo)
		{
			if (!Parent.IMT_SetProductAreaByMenuItem)
			{
				var fields = new[] { Parent.IMT_Product, Parent.IMT_ProductArea, Parent.IMT_Module };
				if (!fields.Skip(1).All(x => x.IsEmpty) && !fields.All(x => !x.IsEmpty))
				{
					propertyInfo.AddError(Res.GetString("08765b2b-e964-4091-bd0e-4f1180069db5", "Product, Product Area, and {0} settings cannot be partially populated. You can either fill in all three fields, leave them all empty, or provide a value only for the Product field.", "Sec./Svc./Req."));
				}
			}
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidatePublishedDescriptionText();
		}

		#region Published Description

		public void ValidatePublishedDescriptionText()
		{
			ValidateCalculatedProperty(Parent.PublishedDescriptionTextInfo);
		}

		protected void CheckPublishedDescriptionText()
		{
			if (Parent.IMT_IsPublished)
			{
				MandatoryValidation.CheckEntered(Parent.PublishedDescriptionTextInfo);
			}
		}

		#endregion
	}
}

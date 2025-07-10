using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentWrappers.GenericWrappers;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	public class CartageAdviceHelper : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Constructor

		public CartageAdviceHelper(IDocCartageAdvice cartageAdviceParent, BusinessObjectFactory factory)
			: base(factory)
		{
			IsAir = cartageAdviceParent.IsAir;
			CountryCode = cartageAdviceParent.CurrentCompany.Country.Code;
		}

		public CartageAdviceHelper(CartageInfoWrapper cartageInfoParent, BusinessObjectFactory factory)
			: base(factory)
		{
			IsAir = cartageInfoParent.IsAir;
			CountryCode = cartageInfoParent.CurrentCompany.Country.Code;
		}

		#endregion

		#region DateAsUniqueIdentifier

		public ZString DateAsUniqueIdentifier
		{
			get { return (IsAir && CountryCode == Constants.CountryCodes.UnitedKingdom) ? ZDateTime.Now.ToString("yyyyMMddHHmmss") : ""; }
		}

		ZBool IsAir { get; set; }

		ZString CountryCode { get; set; }

		#endregion

		#region Headings

		public ZString PickupDateHeading
		{
			get { return Res.GetString("5ee8ad73-49c9-4b0b-945f-468125e56171", "PICKUP DATE"); }
		}

		public ZString ReceivalsStartHeading
		{
			get { return Res.GetString("d633970d-6797-40da-bd5b-9b113019ad58", "RECEIVALS START"); }
		}

		public ZString StorageCommencesHeading
		{
			get { return Res.GetString("127c923b-0a7e-443b-b028-f77eedb70bd9", "STORAGE STARTS"); }
		}

		#endregion
	}
}

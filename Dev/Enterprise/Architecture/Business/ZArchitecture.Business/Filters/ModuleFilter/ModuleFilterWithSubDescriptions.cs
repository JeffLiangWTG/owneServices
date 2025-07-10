using System;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.ZArchitecture.Business
{
	public abstract class ModuleFilterWithSubDescriptions : ModuleFilter
	{
		#region Construction

		protected ModuleFilterWithSubDescriptions(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
		}

		protected ModuleFilterWithSubDescriptions(ZString description)
			: base(description)
		{
		}

		public ModuleFilterWithSubDescriptions(ZString description, SchemaColumn filterColumn)
			: base(description, filterColumn)
		{
		}

		public ModuleFilterWithSubDescriptions(ZString description, Delegate queryDelegate)
			: base(description, queryDelegate)
		{
		}

		#endregion

		#region Item Descriptions

		public void SetItemDescriptions(ResourceStringData description1, ResourceStringData description2)
		{
			if (description1.IsEmpty() || description2.IsEmpty())
			{
				throw new ArgumentException(Description + " filter Item Descriptions cannot be empty.");
			}

#if DEBUG
			if (Res.CurrentLanguage == Res.DefaultLanguage &&
				(description1.Caption.Length > MaxFilterSubDescriptionLength || description2.Caption.Length > MaxFilterSubDescriptionLength))
			{
				throw new ArgumentOutOfRangeException(Description + " filter Item Descriptions cannot be greater than " + MaxFilterSubDescriptionLength + " characters in length. Or they will not fit visually. Note these still need to be translated.");
			}
#endif
			ItemDescription1 = description1;
			ItemDescription2 = description2;
		}

		public ResourceStringData ItemDescription1
		{
			get;
			private set;
		}

		public ResourceStringData ItemDescription2
		{
			get;
			private set;
		}

		public const int MaxFilterSubDescriptionLength = 15;

		#endregion
	}
}

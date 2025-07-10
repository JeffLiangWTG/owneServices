using System;
using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[DefaultField("CodeAndDescription")]
	public class CodeAndDescriptionWrapper : GenericWrapper
	{
		#region Constructors

		protected CodeAndDescriptionWrapper(BusinessObject businessObjectToWrap, BusinessObjectFactory factory) : base(businessObjectToWrap, factory) { }
		public CodeAndDescriptionWrapper(ZString code, IBusinessObjectCollection list, BusinessObjectFactory factory) : this(code, (IList)list, factory) { }
		public CodeAndDescriptionWrapper(ZString code, CodeDescriptionPairList list, BusinessObjectFactory factory) : this(code, (IList)list, factory) { }
		public CodeAndDescriptionWrapper(ZString code, ICodeDescriptionPairList list, BusinessObjectFactory factory) : this(code, (IList)list, factory) { }
		public CodeAndDescriptionWrapper(ZString newCode, CodeAndDescriptionWrapper sourceWrapper) : this(newCode, sourceWrapper.fList, sourceWrapper.Factory) { }

		public CodeAndDescriptionWrapper(ZString code, ZString description, BusinessObjectFactory factory)
			: this(code, factory)
		{
			fDescription = description;
		}

		protected CodeAndDescriptionWrapper(ZString code, BusinessObjectFactory factory)
			: base(null, factory)
		{
			fCode = code;
		}

		protected CodeAndDescriptionWrapper(ZString code, IList list, BusinessObjectFactory factory)
			: base(null, factory)
		{
			if (list == null)
			{
				throw new ArgumentNullException(nameof(list), "A " + GetType().Name + " requires a valid List to be passed into the constructor.");
			}

			fCode = code;
			fList = list;
		}

		#endregion

		internal static CodeAndDescriptionWrapper Empty
		{
			get { return new CodeAndDescriptionWrapper(ZString.Empty, new CodeDescriptionPairList(), null); }
		}

		public ZString Code
		{
			get { return fCode; }
		}

		public ZString Description
		{
			get
			{
				if (fList != null && (fDescription == null || descriptionLanguage != Res.CurrentLanguage))
				{
					descriptionLanguage = Res.CurrentLanguage;
					fDescription = "";
					ICodeDescriptionPairList codeDescriptionPairList = fList as ICodeDescriptionPairList;
					if (codeDescriptionPairList != null)
					{
						fDescription = codeDescriptionPairList.GetDescriptionFromCode(fCode);
					}
					else
					{
						IFindBoxListProvider businessObjectCollection = fList as IFindBoxListProvider;
						if (businessObjectCollection != null)
						{
							fDescription = businessObjectCollection.DescriptionFromCode(fCode);
						}
					}
					if (string.IsNullOrEmpty(fDescription))
					{
						fDescription = fCode;
					}
				}
				return fDescription;
			}
		}

		public ZString CodeAndDescription
		{
			get { return Code + (Code != Description ? (Code.IsEmpty || Description.IsEmpty ? "" : " - ") + Description : ""); }
		}

		#region Implenmentation

		readonly ZString fCode;
		readonly IList fList;

		public CodeDescriptionPairList List
		{
			get { return fList as CodeDescriptionPairList; }
		}

		string fDescription;
		string descriptionLanguage;
		#endregion
	}
}

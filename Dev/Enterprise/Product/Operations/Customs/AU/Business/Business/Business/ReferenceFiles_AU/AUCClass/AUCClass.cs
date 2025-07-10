using System.Collections;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	using System.ComponentModel;

	[DescriptionProperty(AutoAUCClass.Schema.UJ_STDesc)]
	public class AUCClass : AutoAUCClass, IFamilyMember, ITraversibleNode
	{
		/// <summary>
		/// Australian Customs Import Classification
		/// </summary>
		public AUCClass(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override bool SupportsNotes
		{
			get { return false; }
		}

		public static AUCClass GetClassForPartialCode(BusinessObjectFactory factory, ZString code)
		{
			AUCClass result = null;

			if (!code.IsEmpty)
			{
				if (code.Length == 2)   // Special case for subchapter support
				{
					result = factory.LoadTop1<AUCClass>(new ZQuery(AUCClassSchema.UJ_Code, code));
				}
				else
				{
					result = GetAUClassForCode(factory, code);

					if (result == null && code.Length == 13)
					{
						result = GetAUClassForCode(factory, code.Substring(0, 10));
					}
				}
			}

			return result;
		}

		static AUCClass GetAUClassForCode(BusinessObjectFactory factory, ZString code)
		{
			BusinessObject[] result = factory.Load(typeof(AUCClass), new ZQuery(AUCClassSchema.UJ_Code, code));
			return result.Length == 1 ? (AUCClass)result[0] : null;
		}

		public static AUCClass GetClassForCompleteCode(BusinessObjectFactory factory, ZString code)
		{
			if (code.Length == 13)
			{
				return GetClassForPartialCode(factory, code);
			}
			else
			{
				return null;
			}
		}

		public AUCClass ParentClass
		{
			get { return (AUCClass)Factory.Load(typeof(AUCClass), UJ_UJ); }
		}

		//		public bool QuantityRequired
		//		{
		//			get
		//			{
		//				return UJ_UQ1 != "";
		//			}
		//		}
		//

		public override ZString UJ_Code
		{
			get { return base.UJ_Code; }
			set
			{
				base.UJ_Code = value;
				importDescription = null;
			}
		}

		string importDescription;
		public ZString ImportDescription
		{
			get
			{
				if (importDescription == null)
				{
					importDescription = new ImportDescriptionFormatter(Factory, UJ_Code).Description;
					if (string.IsNullOrEmpty(importDescription))
					{
						string firstSixDigits = UJ_Code.Left(7);//left 7 because of decimal point
						ZQuery filter = new ZQuery();
						var myAHECC = Factory.LoadFromNaturalKey<AUCAHECC>(AUCAHECCSchema.UA_AHECC, firstSixDigits);
						if (myAHECC != null)
						{
							importDescription = myAHECC.UA_LongDescription;
						}
					}
				}
				return importDescription;
			}
		}

		#region Implementation
		protected BusinessObject fMaster;
		#endregion

		#region IFamilyMember Members

		public string ShortDescription
		{
			get { return UJ_Code + " " + UJ_Txt; }
		}

		public ZString LongDescription
		{
			get { return UJ_Txt.Trim(); }
		}

		ZPropertyInfo IFamilyMember.LongDescriptionInfo
		{
			get { return UJ_TxtInfo; }
		}

		public IFamilyMember[] Children
		{
			get { return (IFamilyMember[])AUCClasses.ToArray(typeof(IFamilyMember)); }
		}

		public bool HasChildren
		{
			get { return UJ_Code.Trim().Length < 13; }
		}

		public string NodeKey
		{
			get { return UJ_Code; }
		}

		#endregion

		#region AUCClasses

		AUCClassCollection fAUCClasses;
		public AUCClassCollection AUCClasses
		{
			get
			{
				if (fAUCClasses == null)
				{
					fAUCClasses = new AUCClassCollection(this, Factory);
					fAUCClasses.Load();
					fAUCClasses.Sort(AUCClass.Schema.UJ_Code, ListSortDirection.Ascending);
					fAUCClasses.IsManagedForDataRefresh = true;
				}
				return fAUCClasses;
			}
		}

		#endregion

		#region ICodeDescriptionProvider Members

		public ZPropertyInfo CodeInfo
		{
			get
			{
				return UJ_CodeInfo;
			}
		}

		public ZPropertyInfo DescriptionInfo
		{
			get
			{
				return UJ_TxtInfo;
			}
		}

		#endregion

		#region ITraversibleNode Members

		public IFamilyMember[] GetHierarchy()
		{
			ArrayList list = new ArrayList();
			list.Add(this);
			AUCClass currentAUCClass = this;
			while (currentAUCClass.UJ_UJ.IsValid)
			{
				var masterClass = Factory.Load<AUCClass>(currentAUCClass.UJ_UJ);
				if (masterClass != null)
				{
					list.Add(masterClass);
					currentAUCClass = masterClass;
				}
			}

			var chapter = this.Chapter;
			if (chapter != null)
			{
				list.AddRange(chapter.GetHierarchy());
			}
			return (IFamilyMember[])list.ToArray(typeof(IFamilyMember));
		}

		#endregion

		public AUCChapter Chapter
		{
			get
			{
				if (UJ_Code.Length >= 2)
				{
					return Factory.LoadFromNaturalKey<AUCChapter>(AUCChapterSchema.UH_Chapter, UJ_Code.Left(2));
				}
				else
				{
					return null;
				}
			}
		}
	}
}

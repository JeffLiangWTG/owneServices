using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public enum EntryType { Export, Import }

	public class AUCSectionCollection : BusinessObjectCollection<AUCSection>, IFamilyMemberCollection
	{
		public AUCSectionCollection(EntryType entryType)
			: this(new BusinessObjectFactory())
		{
			this.EntryType = entryType;
		}

		public AUCSectionCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public override void Load()
		{
			base.Load();
			Sort(AUCSection.Schema.UG_Section, ListSortDirection.Ascending);
		}

		public override void Add(BusinessObject businessObject)
		{
			base.Add(businessObject);
			((AUCSection)businessObject).EntryType = EntryType;
		}

		public ITraversibleNode GetNearestNodeForCode(ZString code)
		{
			code = code.Trim();
			ITraversibleNode result = null;
			if (code != "")
			{
				if (code.Length <= 2)
				{
					if (code.Length == 1)
					{
						code = "0" + code;
					}
					result = (AUCChapter)Factory.LoadFromNaturalKey(typeof(AUCChapter), AUCChapterSchema.UH_Chapter, code);
				}
				else
				{
					DynamicBusinessObjectCollection top1 = new DynamicBusinessObjectCollection(Factory);
					ZQuery sQLFilter = new ZQuery();
					if (EntryType == EntryType.Export)
					{
						sQLFilter.AddToFilter(AUCAHECCSchema.UA_AHECC, SQLComparisonOperator.StartsWith, code.Left(AUCAHECC.Schema.UA_AHECCMaxLength));
						sQLFilter.OrderBy = "len(" + AUCAHECC.Schema.UA_AHECC + ")";

						top1.Load("select top 1" + AUCAHECC.Schema.PK + " from " + AUCAHECC.Schema.TableName + sQLFilter.GetAsWhereAndOrderByClause(false), sQLFilter.Params);
						if (top1.Count > 0)
						{
							ZGuid pK = (ZGuid)top1[0][AUCAHECC.Schema.PK];
							result = (AUCAHECC)Factory.Load(typeof(AUCAHECC), pK);
						}
					}
					else
					{
						sQLFilter.AddToFilter(AUCClassSchema.UJ_Code, SQLComparisonOperator.StartsWith, code.Left(AUCClass.Schema.UJ_CodeMaxLength));
						sQLFilter.OrderBy = "len(" + AUCClass.Schema.UJ_Code + ")";
						top1.Load("select top 1" + AUCClass.Schema.PK + " from " + AUCClass.Schema.TableName + sQLFilter.GetAsWhereAndOrderByClause(false), sQLFilter.Params);
						if (top1.Count > 0)
						{
							ZGuid pK = (ZGuid)top1[0][AUCClass.Schema.PK];
							result = (AUCClass)Factory.Load(typeof(AUCClass), pK);
						}
					}
				}
			}
			return result;
		}

		public readonly EntryType EntryType;

		//		#region Implementation
		//
		//		protected AUCSection GetSectionForChapter(string ChapterToFind)
		//		{
		//			AUCSection Section = null;
		//			var Chapter = Factory.LoadFromNaturalKey<AUCChapter>(AUCChapter.Schema.UH_Chapter, ChapterToFind);
		//			if (Chapter != null)
		//			{
		//				Section = (AUCSection)Factory.Load(typeof(AUCSection), Chapter.PK);
		//			}
		//			return Section;
		//		}
		//		#endregion

		#region IFamilyMemberCollection Members

		public IFamilyMember[] FamilyMembers
		{
			get
			{
				return (IFamilyMember[])this.ToArray(typeof(IFamilyMember));
			}
		}

		#endregion
	}
}

using System.Collections;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	/// <summary>
	/// AUCChapter is shared between Export and Import.
	/// </summary>

	public class AUCChapter : AutoAUCChapter, IFamilyMember, ITraversibleNode
	{
		public AUCChapter(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override bool SupportsNotes
		{
			get { return false; }
		}

		string IFamilyMember.ShortDescription
		{
			get { return UH_Chapter + " " + UH_Description; }
		}

		ZString IFamilyMember.LongDescription
		{
			get { return UH_Description; }
		}

		ZPropertyInfo IFamilyMember.LongDescriptionInfo
		{
			get { return UH_DescriptionInfo; }
		}

		public IFamilyMember[] Children
		{
			get
			{
				return (IFamilyMember[])ChildCollection.ToArray(typeof(IFamilyMember));
			}
		}

		public BusinessObjectCollection ChildCollection
		{
			get
			{
				if (Section != null && Section.EntryType == EntryType.Export)
				{
					return AUCAHECCs;
				}
				else
				{
					return AUCClasses;
				}
			}
		}

		bool IFamilyMember.HasChildren
		{
			get { return true; }
		}

		#region AUCAHECCs

		AUCChapterAHECCCollection fAUCAHECCs;
		protected AUCChapterAHECCCollection AUCAHECCs
		{
			get
			{
				if (fAUCAHECCs == null)
				{
					ZQuery sQLFilter = new ZQuery();
					sQLFilter.AddToFilter(AUCAHECCSchema.UA_AHECC, SQLComparisonOperator.StartsWith, UH_Chapter);
					sQLFilter.AddToFilter(AUCAHECCSchema.UA_UA, null);
					fAUCAHECCs = new AUCChapterAHECCCollection(Factory, sQLFilter);
					if (UH_Chapter != "")
					{
						fAUCAHECCs.Load();
					}
				}
				return fAUCAHECCs;
			}
		}

		#endregion

		#region AUCClasses

		AUCChapterAUCClassCollection fAUCClasses;
		protected AUCChapterAUCClassCollection AUCClasses
		{
			get
			{
				if (fAUCClasses == null)
				{
					ZQuery sQLFilter = new ZQuery();
					sQLFilter.AddToFilter(AUCClassSchema.UJ_Code, SQLComparisonOperator.StartsWith, UH_Chapter);
					sQLFilter.AddToFilter(AUCClassSchema.UJ_UJ, null);
					fAUCClasses = new AUCChapterAUCClassCollection(Factory, sQLFilter);
					if (UH_Chapter != "")
					{
						fAUCClasses.Load();
					}
				}
				return fAUCClasses;
			}
		}

		#endregion

		public AUCSection Section
		{
			get { return (AUCSection)Factory.Load(typeof(AUCSection), UH_UG); }
		}

		#region IAHECCParticipant Members

		public IFamilyMember[] GetHierarchy()
		{
			ArrayList list = new ArrayList();
			list.Add(this);
			list.AddRange(this.Section.GetHierarchy());
			return (IFamilyMember[])list.ToArray(typeof(IFamilyMember));
		}

		#endregion
	}
}

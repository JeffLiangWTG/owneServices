using System.Collections;
using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	/// <summary>
	/// Australian Harmonised Export Commodity Code
	/// Used only for Export Declarations
	/// </summary>

	[CodeProperty(AutoAUCAHECC.Schema.UA_AHECC), DescriptionProperty(AutoAUCAHECC.Schema.UA_ShortDescription)]
	public class AUCAHECC : AutoAUCAHECC, IFamilyMember, ITraversibleNode
	{
		public AUCAHECC(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override bool SupportsNotes
		{
			get { return false; }
		}

		string IFamilyMember.ShortDescription
		{
			get { return UA_AHECC.Trim() + " " + UA_ShortDescription.Trim(); }
		}

		ZString IFamilyMember.LongDescription
		{
			get { return UA_LongDescription.Trim(); }
		}

		ZPropertyInfo IFamilyMember.LongDescriptionInfo
		{
			get { return UA_LongDescriptionInfo; }
		}

		IFamilyMember[] IFamilyMember.Children
		{
			get { return (IFamilyMember[])AUCAHECCs.ToArray(typeof(IFamilyMember)); }
		}

		public bool HasChildren
		{
			get { return UA_AHECC.Trim().Length < 10; }
		}

		#region AUCAHECCs

		AUCAHECCCollection fAUCAHECCs;
		public AUCAHECCCollection AUCAHECCs
		{
			get
			{
				if (fAUCAHECCs == null)
				{
					fAUCAHECCs = new AUCAHECCCollection(this, Factory);
					fAUCAHECCs.Load();
					fAUCAHECCs.Sort(AUCAHECC.Schema.UA_Index, ListSortDirection.Ascending);
					fAUCAHECCs.IsManagedForDataRefresh = true;
				}
				return fAUCAHECCs;
			}
		}

		#endregion

		public AUCAHECC Master
		{
			get { return (AUCAHECC)Factory.Load(typeof(AUCAHECC), UA_UA); }
		}

		public AUCChapter Chapter
		{
			get
			{
				if (UA_AHECC.Length >= 2)
				{
					return (AUCChapter)Factory.LoadFromNaturalKey(typeof(AUCChapter), AUCChapterSchema.UH_Chapter, UA_AHECC.Left(2));
				}
				else
				{
					return null;
				}
			}
		}

		public static AUCAHECC GetClassForCode(BusinessObjectFactory factory, ZString code)
		{
			return factory.LoadTop1<AUCAHECC>(new ZQuery(ZArchitecture.Schema.AUCAHECCSchema.UA_AHECC, code));
		}

		#region ITaversibleNode Members

		public IFamilyMember[] GetHierarchy()
		{
			ArrayList list = new ArrayList();
			list.Add(this);
			AUCAHECC currentAHECC = this;
			while (currentAHECC.Master != null)
			{
				list.Add(currentAHECC.Master);
				currentAHECC = currentAHECC.Master;
			}

			var chapter = this.Chapter;
			if (chapter != null)
			{
				list.AddRange(chapter.GetHierarchy());
			}

			return (IFamilyMember[])list.ToArray(typeof(IFamilyMember));
		}
		#endregion
	}
}

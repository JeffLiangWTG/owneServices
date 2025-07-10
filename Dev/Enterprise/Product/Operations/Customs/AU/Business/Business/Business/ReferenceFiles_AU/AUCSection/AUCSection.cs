using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AUCSection : AutoAUCSection, IFamilyMember, ITraversibleNode
	{
		public AUCSection(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public EntryType EntryType;

		string IFamilyMember.ShortDescription
		{
			get { return UG_Section.ToString() + " " + UG_Description; }
		}

		ZString IFamilyMember.LongDescription
		{
			get { return UG_Description; }
		}

		ZPropertyInfo IFamilyMember.LongDescriptionInfo
		{
			get { return UG_DescriptionInfo; }
		}

		IFamilyMember[] IFamilyMember.Children
		{
			get { return (IFamilyMember[])AUCChapters.ToArray(typeof(IFamilyMember)); }
		}

		bool IFamilyMember.HasChildren
		{
			get { return true; }
		}

		#region AUCChapters

		AUCChapterCollection fAUCChapters;
		public AUCChapterCollection AUCChapters
		{
			get
			{
				if (fAUCChapters == null)
				{
					fAUCChapters = new AUCChapterCollection(this, Factory);
					fAUCChapters.Load();
					fAUCChapters.IsManagedForDataRefresh = true;
				}
				return fAUCChapters;
			}
		}

		#endregion

		#region IAHECCNode Members

		public IFamilyMember[] GetHierarchy()
		{
			return new IFamilyMember[] { this };
		}

		#endregion
	}
}

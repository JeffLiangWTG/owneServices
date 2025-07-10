using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public interface IFamilyMember
	{
		string ShortDescription
		{
			get;
		}

		ZString LongDescription
		{
			get;
		}

		ZPropertyInfo LongDescriptionInfo
		{
			get;
		}

		IFamilyMember[] Children
		{
			get;
		}

		bool HasChildren
		{
			get;
		}
	}

	public interface IFamilyMemberCollection
	{
		ITraversibleNode GetNearestNodeForCode(ZString code);
		IFamilyMember[] FamilyMembers { get; }
	}
}

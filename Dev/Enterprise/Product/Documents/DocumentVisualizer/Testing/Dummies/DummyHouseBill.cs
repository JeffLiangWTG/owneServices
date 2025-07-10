using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class DummyHouseBill : DocDataObject, IHouseBill
	{
		public ZDateTime DateOfIssue
		{
			get => dateOfIssue;
			set
			{
				if (SetNonPersistentPropertyValue(DateOfIssueInfo, ref dateOfIssue, value))
				{
				}
			}
		}

		ZDateTime dateOfIssue;

		public ZPropertyInfo DateOfIssueInfo => GetZPropertyInfo(nameof(DateOfIssue));
	}
}

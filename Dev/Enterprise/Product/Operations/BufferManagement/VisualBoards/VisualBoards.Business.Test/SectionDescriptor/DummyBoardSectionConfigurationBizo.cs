using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;

namespace Enterprise.VisualBoards.Business.Test
{
	public class DummyBoardSectionConfigurationBizo : DummyNonPersistentBusinessObject, IBoardSectionConfigurationBizo
	{
		public ZString SectionName
		{
			get { return string.Empty; }
			set { }
		}

		public ZPropertyInfo SectionNameInfo
		{
			get { return GetZPropertyInfo(nameof(SectionName)); }
		}

		public void CopyConfigurationPropertiesToNewSection(IBMBoardSection section)
		{
		}

		public override void OnSaving()
		{
			// Don't call base because that requires a factory, which we don't have in this dummy class. Why is OnSaving used for non-persistent bizos anyway???
		}
	}
}

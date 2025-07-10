using CargoWise.PAVE.Common.DTO;
using Enterprise.BufferManagement.Business;

namespace Enterprise.BufferManagement.Service
{
	internal class SectionConfigurationBuilder : ISectionConfigurationBuilder
	{
		internal SectionConfigurationBuilder(BMBoardSection section, SectionType type)
		{
			Section = section;
			Type = type;
		}

		protected BMBoardSection Section { get; }
		SectionType Type { get; }

		public ISectionConfiguration Build()
		{
			return BuildCore();
		}

		protected virtual ISectionConfiguration BuildCore()
		{
			return new SectionConfigurationDTO()
			{
				SectionPK = Section.PK.ToGuid(),
				Type = Type,
				Name = Section.SectionName,
				Layout = CreateLayout()
			};
		}

		#region Layout

		protected SectionLayoutDTO CreateLayout()
		{
			return new SectionLayoutDTO
			{
				Row = Section.Row,
				Column = Section.Column,
				RowSpan = Section.RowSpan,
				ColSpan = Section.ColSpan,
				RowHeightPercent = Section.RowHeightPercent,
				ColWidthPercent = Section.ColWidthPercent
			};
		}

		#endregion

	}
}

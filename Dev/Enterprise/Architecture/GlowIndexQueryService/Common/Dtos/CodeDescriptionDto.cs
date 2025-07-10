namespace GlowIndexQueryService.Common.Dtos
{
	class CodeDescriptionDto
	{
		public CodeDescriptionDto(string code, string description)
		{
			Code = code;
			Description = description;
		}
		public string Code { get; }
		public string Description { get; }
	}
}

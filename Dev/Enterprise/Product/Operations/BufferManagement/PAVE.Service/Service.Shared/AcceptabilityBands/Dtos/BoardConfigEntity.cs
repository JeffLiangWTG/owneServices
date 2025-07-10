using System;

namespace Enterprise.BufferManagement.Service.Shared
{
	public class BoardConfigEntity
	{
		public Guid Id { get; set; }
		public string Name { get; set; } = null!;
		public string Description { get; set; } = null!;
	}
}

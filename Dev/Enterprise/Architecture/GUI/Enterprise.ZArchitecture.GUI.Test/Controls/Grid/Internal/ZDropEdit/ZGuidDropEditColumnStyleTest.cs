using System;
using CargoWise.Integration;

namespace Enterprise.ZArchitecture.GUI
{
	class TestGetCodeForNonZGuidListItemPK_TestingClass : ICodeDescription
	{
		public string PKReturns { get; set; } = string.Empty;
		public object PK => PKReturns;
		public string Code => throw new NotImplementedException();
		public string Description => throw new NotImplementedException();
	}
}

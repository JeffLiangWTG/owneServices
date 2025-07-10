using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI.Internal.Testing
{
	sealed class DummyWithList : NonPersistentBusinessObject
	{
		[List("SomeCode_List")]
		public ZString SomeCode { get; set; }

		public CodeDescriptionPairList SomeCode_List
		{
			get
			{
				var list = new CodeDescriptionPairList();
				list.AddPair("", "");
				list.Add(new CategoryCodeDescriptionPair("Some Category", ""));
				list.AddPair("ABC", "Alpabet");

				list.AddPair("", "");
				list.Add(new CategoryCodeDescriptionPair("Another Category", ""));
				list.AddPair("AABC", "Alpabet");
				list.AddPair("DEF", "Alpabet");
				list.AddPair(" SPACE", "Alpabet");

				return list;
			}
		}

		[List("NoCategoryCode_List")]
		public ZString NoCategoryCode { get; set; }

		public CodeDescriptionPairList NoCategoryCode_List
		{
			get
			{
				var list = new CodeDescriptionPairList();
				list.AddPair("", "");
				list.AddPair("ABC", "Alpabet");

				list.AddPair("", "");
				list.AddPair("AABC", "Alpabet");
				list.AddPair("DEF", "Alpabet");

				return list;
			}
		}
	}
}

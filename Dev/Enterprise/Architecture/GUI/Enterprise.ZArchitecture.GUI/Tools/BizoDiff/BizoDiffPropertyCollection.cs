using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.DevTools
{
	public sealed class BizoDiffPropertyCollection : NonPersistentBusinessObjectCollection<BizoProperty>
	{
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotImplementedException();
		}

		public List<string> KeyFields => Where(v => v.KeyField).Select(v => (string)v.Name).ToList();
		public List<string> IgnoreFields => Where(v => v.IgnoreField).Select(v => (string)v.Name).ToList();

		protected override bool AllowNewCore => false;
	}
}

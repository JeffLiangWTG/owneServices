using System.Collections.Generic;
using CargoWise.Application;

namespace Enterprise.ZArchitecture.GUI
{
	public class SafeClipboardHelper
	{
		public static SafeClipboardHelper Instance
		{
			get
			{
				if (ObjectFactory.Contains(nameof(SafeClipboardHelper)))
				{
					return ObjectFactory.Get<SafeClipboardHelper>();
				}
				return new SafeClipboardHelper();
			}
		}
		public virtual Dictionary<string, string> GetDataObjectDict() => SafeClipboard.GetDataObjectDict();
	}
}

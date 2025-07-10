using System;
using Enterprise.DocumentScanning.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentScanning.GUI
{
	public class SeparatorMenuItem : eDocMenuItem
	{
		public SeparatorMenuItem(EventHandler onClickHandler)
			: base((NoResString)Constants.SeparatorMenuText, onClickHandler)
		{
		}

		public SeparatorMenuItem()
			: this(null)
		{
		}

		public override bool GetEnabledStatus(StorageDocsBase selectedElement, bool multipleSelected)
		{
			return true;
		}
	}
}

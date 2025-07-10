using System;

namespace Enterprise.Loader
{
	interface IIconCreationDialog : IDisposable
	{
		void ShowDialog();
		void SetInstanceDescription(string description, bool isReadonly);
		IconCreationOptions IconCreationOptions { get; }
	}
}

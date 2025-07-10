using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Layout;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI
{
	public interface ISplitterLayoutProvider
	{
		bool RememberSplitterLayout { get; set; }
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		Dictionary<string, (ISplitterLayoutSaveProvider Splitter, int SplitterPosition)> Splitters { get; }
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
	public interface IAcceptNegativeSplitterPosition { }

	public static class SplitterLayoutStrategy
	{
		public static void RestoreSplittersLayout(Control control)
		{
			if (!control.IsDesignMode() && !Globals.IsTest && control is ISplitterLayoutProvider splitterLayoutProvider && splitterLayoutProvider.RememberSplitterLayout)
			{
				RestoreSplittersLayoutCore(control);
			}
		}

#if DEBUG
		public static void RestoreSplittersLayoutForTest(Control control)
		{
			RestoreSplittersLayoutCore(control);
		}
#endif

		static void ControlsAdded(object sender, ControlEventArgs e)
		{
			RestoreSplittersLayoutCore(e.Control);
		}

#if DEBUG
		internal
#endif
		static void RestoreSplittersLayoutCore(Control control)
		{
			control.ControlAdded -= ControlsAdded;
			control.Paint -= Control_Paint;
			control.ControlAdded += ControlsAdded;
			control.Paint += Control_Paint;

			foreach (Control subControl in control.Controls)
			{
				RestoreSplittersLayoutCore(subControl);
			}
		}

		static void Control_Paint(object sender, PaintEventArgs e)
		{
			if (sender is ISplitterLayoutSaveProvider splitter && !splitter.IsLayoutRestored && !splitter.IsSplitterFixed)
			{
				var key = ComputeUniqueKeyForSplitter((Control)splitter);
				var splitterPositionScale = ((IWinFormsEnvironment)EnvProxy.Instance).SplitterLayoutRegistry.GetSplitterLayout(key);
				var splitterPosition = (int)(splitter.ContainerSize * splitterPositionScale);

				splitter.SplitterPosition = (splitterPosition > 0 || splitter is IAcceptNegativeSplitterPosition) ? splitterPosition : splitter.SplitterPosition;
				splitter.IsLayoutRestored = true;

				StoreSplittersStatus(key, splitter);
			}
		}

		internal static string ComputeUniqueKeyForSplitter(Control control)
		{
			using var md5 = MD5.Create();

			var outputBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(GetFullAccessPathOfControl(control)));
			var key = "SplitterLayout|";

			for (var i = 0; i < outputBytes.Length; i++)
			{
				key += outputBytes[i].ToString("x2", CultureInfo.InvariantCulture);
			}

			return key;
		}

		static string GetFullAccessPathOfControl(Control control)
		{
			if (control.Parent != null)
			{
				return GetFullAccessPathOfControl(control.Parent) + "|" + control.Name;
			}

			return control.Name;
		}

		static void StoreSplittersStatus(string key, ISplitterLayoutSaveProvider splitter)
		{
			if (((Control)splitter).TopLevelControl is ISplitterLayoutProvider splitterLayoutProvider && !splitterLayoutProvider.Splitters.ContainsKey(key))
			{
				splitterLayoutProvider.Splitters.Add(key, (splitter, splitter.SplitterPosition));
			}
		}

		public static void SaveSplittersLayout(Form form)
		{
			if (form is ISplitterLayoutProvider splitterLayoutProvider)
			{
				foreach (var item in splitterLayoutProvider.Splitters)
				{
					if (splitterLayoutProvider.RememberSplitterLayout && !item.Value.Splitter.IsSplitterFixed && item.Value.Splitter.SplitterPosition != item.Value.SplitterPosition)
					{
						var splitterPositionScale = (item.Value.Splitter.ContainerSize > 0 || item.Value.Splitter is IAcceptNegativeSplitterPosition)
							? decimal.Round(item.Value.Splitter.SplitterPosition / (decimal)item.Value.Splitter.ContainerSize, 2)
							: 0M;
						((IWinFormsEnvironment)EnvProxy.Instance).SplitterLayoutRegistry.SetSplitterLayout(item.Key, splitterPositionScale);
					}
				}
			}
		}
	}
}

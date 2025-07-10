using System;
using System.Globalization;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.MasterFiles.GUI
{
	public class SplitterState
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
		public static void Persist(SplitContainer splitter)
		{
			if (!DesignModeFinder.IsDesigning)
			{
				_ = new SplitterState(splitter);
			}
		}

		readonly SplitContainer splitter;
		string splitterName;

		SplitterState(SplitContainer splitter)
		{
			this.splitter = splitter;

			// Setting the SplitterDistance is not reliable unless done asynchronously
			splitter.BeginInvoke(new MethodInvoker(delegate
				{
					splitterName = splitter.Name;
					Restore(splitter);
					Form form = splitter.ParentForm;
					form.FormClosing += new FormClosingEventHandler(form_FormClosing);
				}
			));
		}

		void form_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (splitter == null)
			{
				ErrorReporter.ReportOnce(string.Format(CultureInfo.InvariantCulture, "An exception occurred: splitter is null. splitter Name: {0}, ParentForm Name: {1}", splitterName, ((Form)sender).Name));
				return;
			}

			Save(splitter);
		}

		static int GetDistance(SplitContainer splitter)
		{
			int size = OrientationSize(splitter);
			// Store as percentage if FixedPanel.None 
			int distanceToStore =
				 splitter.FixedPanel == FixedPanel.Panel1 ? splitter.SplitterDistance :
				 splitter.FixedPanel == FixedPanel.Panel2 ? size - splitter.SplitterDistance :
				 size != 0 ? (splitter.SplitterDistance * 100 + size / 2) / size : 0;

			return distanceToStore;
		}

		static void SetDistance(SplitContainer splitter, int storedDistance)
		{
			// calculate splitter distance with regard to current control size 
			int size = OrientationSize(splitter);
			int distanceToRestore =
				 splitter.FixedPanel == FixedPanel.Panel1 ? storedDistance :
				 splitter.FixedPanel == FixedPanel.Panel2 ? size - storedDistance :
				 storedDistance * size / 100;

			distanceToRestore = Math.Min(Math.Max(distanceToRestore, splitter.Panel1MinSize), size - splitter.Panel2MinSize);

			if (distanceToRestore >= 0)
			{
				try
				{
					splitter.SplitterDistance = distanceToRestore;
				}
				catch (InvalidOperationException)
				{
				}
			}
		}

		static int OrientationSize(SplitContainer splitter)
		{
			return splitter.Orientation == Orientation.Horizontal ? splitter.Height : splitter.Width;
		}

		static void Save(SplitContainer splitter)
		{
			var reg = EDIDataRegistry.Instance.SplitterDistance;
			reg.Name = FullName(splitter);
			int val = GetDistance(splitter);
			if (val != 0)
			{
				reg.SetValue(Env.CurrentUser.PK, Guid.Empty, Guid.Empty, GetDistance(splitter));
			}
		}

		static void Restore(SplitContainer splitter)
		{
			var reg = EDIDataRegistry.Instance.SplitterDistance;
			reg.Name = FullName(splitter);
			int storedDistance = reg.GetValueWithoutFallback(Env.CurrentUser.PK, Guid.Empty, Guid.Empty);
			if (storedDistance != -1)
			{
				SetDistance(splitter, storedDistance);
			}
		}

		static string FullName(SplitContainer splitter)
		{
			return splitter.ParentForm?.Name + "." + splitter.Name;
		}
	}
}

using System;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	public class ProgressFormWrapperTest : TestCase
	{
		public void TestProgressFormWrapperForMultiStep()
		{
			var dummyBizo1 = new DummyProgressFormSupportableBizOType1();
			var dummyBizo2 = new DummyProgressFormSupportableBizOType();

			using (wrapper = new DummyProgressFormWrapper(false, true, dummyBizo1, dummyBizo2))
			{
				wrapper.AssertStatus += wrapper_AssertStatus;
				wrapper.ShowProgressForm();
				dummyBizo1.ProcessTask();
				dummyBizo2.ProcessTask();
				AssertType(typeof(MultistepProgressForm), wrapper.ProgressForm);
			}
		}

		public void TestProgressFormWrapperForSingleStep()
		{
			var dummyBizo1 = new DummyProgressFormSupportableBizOType1();

			using (wrapper = new DummyProgressFormWrapper(false, true, dummyBizo1))
			{
				wrapper.AssertStatus += wrapper_AssertStatus;
				wrapper.ShowProgressForm();
				dummyBizo1.ProcessTask();
				AssertType(typeof(ProgressForm), wrapper.ProgressForm);
			}
		}

		void wrapper_AssertStatus(object sender, EventArgs e)
		{
			AssertNotNull("Object must be of Type IProgressFormSupportable", (sender as IProgressFormSupportable));
			AssertEndsWith("Current Status of Progress Form", (sender as IProgressFormSupportable).CurrentStatusText, wrapper.ProgressForm.Status);
		}

		public class DummyProgressFormWrapper : ProgressFormWrapper, IDisposable
		{
			public DummyProgressFormWrapper(bool allowCancel, bool showFullLog, params IProgressFormSupportable[] progressFormSupportableBizO)
				: base(new ZForm(), new ResourceStringData("8f69838b-a137-4974-9c3a-e19329aae5ac", "Test Progress Wraper"), allowCancel, showFullLog, progressFormSupportableBizO)
			{
			}

			protected override void RaiseProgressUpdateEventHandler(IProgressFormSupportable sender, bool isProcessCompleted)
			{
				base.RaiseProgressUpdateEventHandler(sender, isProcessCompleted);
				if (AssertStatus != null)
				{
					AssertStatus(sender, null);
				}
			}

			public ProgressForm ProgressForm
			{
				get { return progressForm; }
			}

			public string LogLine
			{
				get
				{
					return logs.ToStringWithNewLineBetweenAppends();
				}
			}

			public event EventHandler AssertStatus;

			public new void Dispose()
			{
				base.parentForm.Dispose();
				base.Dispose();
			}
		}

		class DummyProgressFormSupportableBizOType1 : DummyProgressFormSupportableBizOType
		{
			public override string CurrentStatusText
			{
				get
				{
					var msg = string.Empty;
					switch (CompletedItems)
					{
						case 0:
							msg = @"Commencing Step 1
--------------------------------------------------------------
";
							break;
						case 10:
							msg = @"
Step 1 ended.";
							break;
						default:
							msg = "Processed " + CompletedItems.ToString() + " of " + TotaItemsToComplete.ToString();
							break;
					}
					return msg;
				}
			}
		}

		class DummyProgressFormSupportableBizOType : IProgressFormSupportable
		{
			public virtual string CurrentStatusText
			{
				get
				{
					var msg = string.Empty;
					switch (CompletedItems)
					{
						case 0:
							msg = "Hai, Step 2 is starting...";
							break;
						case 10:
							msg = "Step 2 is done.";
							break;
						default:
							msg = "Processed " + CompletedItems.ToString() + " of " + TotaItemsToComplete.ToString();
							break;
					}
					return msg;
				}
			}

			public virtual string Log { get { return CurrentStatusText; } }

			public int CompletedItems
			{
				get { return currentItemNumber; }
			}

			public int TotaItemsToComplete
			{
				get { return 10; }
			}

			public void ProcessTask()
			{
				if (RaiseProgressUpdateEvent != null)
				{
					RaiseProgressUpdateEvent(this, false);
				}

				for (int i = 1; i <= TotaItemsToComplete; i++)
				{
					currentItemNumber++;
					currentTransactionNumber = currentItemNumber.ToString().PadLeft(4, '0');

					if (RaiseProgressUpdateEvent != null)
					{
						RaiseProgressUpdateEvent(this, i == TotaItemsToComplete);
					}
				}
			}

			protected string currentTransactionNumber;
			protected int currentItemNumber;

			public event Action<IProgressFormSupportable, bool> RaiseProgressUpdateEvent;
		}

		DummyProgressFormWrapper wrapper;
	}
}

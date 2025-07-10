using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Common;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Notifications.Testing
{
	sealed class NotificationAdornmentFactoryTest : TestCase
	{
		//If NotificationAdornmentFactory is not thread-safe, this test will crash CW1 (tested 3 times).
		//If NotificationAdornmentFactory is thread-safe, it will pass 10000 times in a row.
		public void TestMultithreadedAccess()
		{
			using (AdornmentFactoryChanger())
			{
				var instance = NotificationAdornmentFactory.Instance;
				NotificationAdornmentFactory instanceInThread1 = null;
				var thread1 = new Thread(() => { instanceInThread1 = NotificationAdornmentFactory.Instance; });
				thread1.Start();
				thread1.Join();
				AssertEquals("Both threads should get same instance", instance, instanceInThread1);
				var thread2 = new Thread(() =>
				{
					for (var i = 0; i < 100; ++i)
					{
						NotificationAdornmentFactory.RegisterCustomLayout(new AdornmentLayout<TextBoxBase>());
						NotificationAdornmentFactory.Instance.GetLayout(typeof(Control));
						NotificationAdornmentFactory.Instance.GetLayout(typeof(TextBox));
					}
				});
				thread2.Start();
				for (var i = 0; i < 100; ++i)
				{
					NotificationAdornmentFactory.RegisterCustomLayout(new AdornmentLayout<TextBoxBase>());
					NotificationAdornmentFactory.Instance.GetLayout(typeof(Control));
					NotificationAdornmentFactory.Instance.GetLayout(typeof(TextBox));
				}
				thread2.Join();
				AssertNotEquals(null, NotificationAdornmentFactory.Instance.GetLayout(typeof(Control)));
				AssertNotEquals(null, NotificationAdornmentFactory.Instance.GetLayout(typeof(TextBox)));
				AssertNotEquals(null, NotificationAdornmentFactory.Instance.GetLayout(typeof(TextBoxBase)));
			}
		}

		public void TestFindsExactLayout()
		{
			using (AdornmentFactoryChanger())
			{
				var specificLayout = new AdornmentLayout<TextBox>();
				var generalLayout = new AdornmentLayout<Control>();

				NotificationAdornmentFactory.RegisterCustomLayout(specificLayout);
				NotificationAdornmentFactory.RegisterCustomLayout(generalLayout);

				Assert(specificLayout == NotificationAdornmentFactory.Instance.GetLayout(typeof(TextBox)));
			}
		}

		public void TestFindsFirstAppopriateLayoutIfNoExactLayoutRegistered()
		{
			using (AdornmentFactoryChanger())
			{
				var generalLayout1 = new AdornmentLayout<Control>();
				var generalLayout2 = new AdornmentLayout<TextBoxBase>();

				NotificationAdornmentFactory.RegisterCustomLayout(generalLayout1);
				NotificationAdornmentFactory.RegisterCustomLayout(generalLayout2);

				Assert(generalLayout1 == NotificationAdornmentFactory.Instance.GetLayout(typeof(TextBox)));
			}
		}

		public void TestUsesDefaultLayoutIfNoSpecificOrAppropriateLayotFound()
		{
			using (AdornmentFactoryChanger())
			{
				Assert(typeof(DefaultAdornmentLayout) == NotificationAdornmentFactory.Instance.GetLayout(typeof(TextBox)).GetType());
			}
		}

		public void TestCreateCompositeAdornmentWithBackgroundAndIconAdornmentsForEachTarget()
		{
			using (AdornmentFactoryChanger())
			{
				NotificationAdornmentFactory.RegisterCustomLayout(new TestAdornmentLayout());

				var control = new TestControl();
				var adornment = NotificationAdornmentFactory.Create(control) as CompositeAdornment;

				Assert(adornment != null);

				Assert(adornment[0] is BackgroundAdornment);
				Assert(adornment[0].Control == control.control1);
				Assert(adornment[1] is BackgroundAdornment);
				Assert(adornment[1].Control == control.control2);

				Assert(adornment[2] is IconAdornment);
				Assert(adornment[2].Control == control.control1);
				//Assert((adornment[2] as IconAdornment).Align == IconAlignment.Center);

				Assert(adornment[3] is IconAdornment);
				Assert(adornment[3].Control == control.control2);
				//Assert((adornment[3] as IconAdornment).Align == IconAlignment.Right);
			}
		}

		#region Support

		static IDisposable AdornmentFactoryChanger()
		{
			var oldInstance = NotificationAdornmentFactory.Instance;
			NotificationAdornmentFactory.Instance = new NotificationAdornmentFactory();

			return new DisposableAction(delegate
			{
				NotificationAdornmentFactory.Instance = oldInstance;
			});
		}

		class TestControl : Control
		{
			public readonly TextBox control1 = new TextBox();
			public readonly CheckBox control2 = new CheckBox();
		}

		class TestAdornmentLayout : AdornmentLayout<TestControl>
		{
			public override IEnumerable<Control> GetBackroundAdornmentTargets(TestControl source)
			{
				yield return source.control1;
				yield return source.control2;
			}

			public override IEnumerable<IIconLayout> GetIconAdornmentTargets(TestControl source)
			{
				yield return new IconLayout(source.control1, IconAlignment.Center);
				yield return new IconLayout(source.control2, IconAlignment.Right);
			}
		}

		#endregion
	}
}

using System;
using System.Reflection;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Integration.Testing
{
	public abstract class BaseBulkJobProfitPrintingModuleTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestPrintingForModule()
		{
			var printingHelper = new Mock<IBulkJobProfitPrintingModuleHelper>(MockBehavior.Strict);
			var testMenuItem = (IMenuItem)Activator.CreateInstance(Type.GetType("CargoWise.Windows.UI.KMenuItem, CargoWise.Windows.UI", true));

			bool eventHandlerAdded = false;

			printingHelper.Setup(m => m.GetMenuItem(It.IsAny<JobProfitPrintingDelegate>()))
					.Returns(new GetMenuItemDelegate(delegate(JobProfitPrintingDelegate jobProfitPrintingDelegate)
					{
						if (!eventHandlerAdded)
						{
							testMenuItem.GetType().GetEvent("Click",
								BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public)
								.AddEventHandler(testMenuItem, new EventHandler(delegate
								{ jobProfitPrintingDelegate(); }));

							eventHandlerAdded = true;
						}
						return testMenuItem;
					}));

			printingHelper.Setup(m => m.PrintJobProfitDocument(It.IsAny<BusinessObjectFactory>(), It.IsAny<BusinessObject[]>()));

			using (
				UseImplementationForConsol ?
				ObjectFactory.Substitute("BulkConsolProfitPrintingModuleHelper", printingHelper.Object) :
				ObjectFactory.Substitute(printingHelper.Object))
			{
				FindAndClickMenuItem(testMenuItem);
				Assert("Fake assert", true);
				printingHelper.VerifyAll();
			}
		}

		protected abstract void FindAndClickMenuItem(IMenuItem testMenuItem);
		protected virtual bool UseImplementationForConsol
		{
			get { return false; }
		}
	}

	public delegate IMenuItem GetMenuItemDelegate(JobProfitPrintingDelegate jobProfitPrintingDelegate);
}

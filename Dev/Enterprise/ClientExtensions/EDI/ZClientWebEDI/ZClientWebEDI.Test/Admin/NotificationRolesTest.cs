using System;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	public class NotificationRolesTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestPageLoad()
		{
			var page = GetPageForTest();
			page.DoPageLoad();
		}

		NotificationRolesForTest GetPageForTest()
		{
			var page = new NotificationRolesForTest();
			return page;
		}

		class NotificationRolesForTest : NotificationRoles
		{
			public void DoPageLoad()
			{
				try
				{
					base.OnLoad(EventArgs.Empty);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					if (ex is QueryStringException)
					{
						throw;
					}
				}
			}
		}
	}
}
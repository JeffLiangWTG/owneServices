using System;
using System.ComponentModel;
using CargoWise.Common;

namespace CargoWise.EntityFramework
{
	internal static class ListChangedDelegateInvoker
	{
		public static void Invoke(ListChangedEventHandler method, object sender, ListChangedEventArgs e)
		{
			Delegate[] invocationList = method.GetInvocationList();
			for (int i = 0; i < invocationList.Length; i++)
			{
				ListChangedEventHandler handler = (ListChangedEventHandler)invocationList[i];
				try
				{
					handler(sender, e);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					i++;
					for (; i < invocationList.Length; i++)
					{
						try
						{
							handler = (ListChangedEventHandler)invocationList[i];
							handler(sender, e);
						}
						catch (Exception ex1) when (!ex1.IsCriticalException())
						{
						}
					}
					throw;
				}
			}
		}
	}
}

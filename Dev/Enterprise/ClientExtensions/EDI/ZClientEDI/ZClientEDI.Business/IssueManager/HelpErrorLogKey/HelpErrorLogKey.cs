using System;
using System.Data;
using System.Globalization;
using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.IssueManager.Business
{
	[DependentBusinessObject(typeof(EdiHelpErrorLog), "Keys")]
	public class HelpErrorLogKey : AutoHelpErrorLogKey
	{
		public HelpErrorLogKey(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public EdiHelpErrorLog Issue
		{
			get { return Factory.Load<EdiHelpErrorLog>(HK_HE); }
		}

		public static int GetKeyHashCode(string key)
		{
			unsafe
			{
				unchecked
				{
					fixed (char* source = key)
					{
						int hash1 = 5381;
						int hash2 = hash1;

						int unicode;
						char* source_unfixed = source;

						while ((unicode = source_unfixed[0]) != 0)
						{
							hash1 = ((hash1 << 5) + hash1) ^ unicode;
							unicode = source_unfixed[1];

							if (unicode == 0)
							{
								break;
							}

							hash2 = ((hash2 << 5) + hash2) ^ unicode;
							source_unfixed += 2;
						}

						return hash1 + (hash2 * 1566083941);
					}
				}
			}
		}

		#region Test Methods
#if DEBUG

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			HK_Key = "Key" + new Random(DateTime.UtcNow.Millisecond).Next().ToString(CultureInfo.InvariantCulture);
			HK_HashCode = GetKeyHashCode(HK_Key);
		}

#endif
		#endregion
	}
}


using System.Text.RegularExpressions;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Accounting.Registry.Business.TransactionNumberSequenceCustomisation;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class TransactionNumberSequenceCustomisationCollection : RegistryBusinessObjectCollectionTemplate
	{
		public TransactionNumberSequenceCustomisationCollection()
			: base()
		{
		}

		public TransactionNumberSequenceCustomisationCollection(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		public new TransactionNumberSequenceCustomisation this[int x]
		{
			get { return (TransactionNumberSequenceCustomisation)base[x]; }
		}

		public TransactionNumberSequenceCustomisation this[string elementName]
		{
			get
			{
				foreach (TransactionNumberSequenceCustomisation element in this)
				{
					if (element.ElementName == elementName)
					{
						return element;
					}
				}

				return null;
			}
		}

		public ZInt TotalLength
		{
			get
			{
				var result = ZInt.Zero;

				foreach (TransactionNumberSequenceCustomisation element in this)
				{
					if (element.Include)
					{
						result += element.Length;
					}
				}

				return result;
			}
		}

		public ZInt TotalLengthInNumeric
		{
			get
			{
				var result = ZInt.Zero;

				foreach (TransactionNumberSequenceCustomisation element in this)
				{
					if (element.Include)
					{
						switch (element.ElementName)
						{
							case ElementNames.TransactionHeaderDepartmentCode:
								result += 6;
								break;
							case ElementNames.TransactionHeaderBranchCode:
								foreach (var item in Env.CurrentBranch.Code.ToCharArray())
								{
									result += Regex.IsMatch(item.ToString(), "^[A-Z]$") ? 2 : 1;
								}
								break;
							case ElementNames.CustomElement1:
							case ElementNames.CustomElement2:
								foreach (var item in element.Code.ToString().ToCharArray())
								{
									result += Regex.IsMatch(item.ToString(), "^[A-Z]$") ? 2 : 1;
								}
								break;
							case ElementNames.YearAsLetter:
							case ElementNames.MonthAsLetter:
							case ElementNames.AccountingYearAsLetter:
								result += 2;
								break;
							default:
								result += element.Length;
								break;
						}
					}
				}

				return result;
			}
		}

		#region Implementation

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		public new TransactionNumberSequenceCustomisation AddNew()
		{
			return (TransactionNumberSequenceCustomisation)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new TransactionNumberSequenceCustomisationCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new TransactionNumberSequenceCustomisation(CurrentFallbackLevel);
		}

		#endregion
	}
}

using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class BillOfLadingNumberCustomisationsByServiceLevel : AutoBillOfLadingNumberCustomisationsByServiceLevel
	{
		#region BillOfLadingNumberCustomisations

		public BillOfLadingNumberCustomisationCollection BillOfLadingNumberCustomisations //By Service Level
		{
			get
			{
				if (billOfLadingNumberCustomisations == null)
				{
					billOfLadingNumberCustomisations = new BillOfLadingNumberCustomisationCollection(this);

					BillOfLadingNumberCustomisation allCustomisation = billOfLadingNumberCustomisations.AddNew();
					allCustomisation.MakeDefaultServiceLevel();

					RegisterEditableChildObject(billOfLadingNumberCustomisations);
				}
				return billOfLadingNumberCustomisations;
			}
		}
		BillOfLadingNumberCustomisationCollection billOfLadingNumberCustomisations;

		#endregion

		#region Categories

		public NumberCustomisationElementCategories Categories
		{
			get { return categories; }
			set
			{
				if (value != categories)
				{
					categories = value;

					foreach (BillOfLadingNumberCustomisation customisation in BillOfLadingNumberCustomisations)
					{
						customisation.Categories = value;
					}
				}
			}
		}
		NumberCustomisationElementCategories categories = NumberCustomisationElementCategories.Default;

		public bool AllowNonAlphanumericCharacters
		{
			get { return allowNonAlphanumericCharacters; }
			set
			{
				if (value != allowNonAlphanumericCharacters)
				{
					allowNonAlphanumericCharacters = value;

					foreach (BillOfLadingNumberCustomisation customisation in BillOfLadingNumberCustomisations)
					{
						customisation.AllowNonAlphanumericCharacters = value;
					}
				}
			}
		}
		bool allowNonAlphanumericCharacters;

		public bool EnableMacroInsertion
		{
			get { return enableMacroInsertion; }
			set
			{
				if (value != enableMacroInsertion)
				{
					enableMacroInsertion = value;

					foreach (BillOfLadingNumberCustomisation customisation in BillOfLadingNumberCustomisations)
					{
						customisation.EnableMacroInsertion = value;
					}
				}
			}
		}
		bool enableMacroInsertion;

		public int PrefixLength
		{
			get { return prefixLength; }
			set
			{
				if (value != prefixLength)
				{
					prefixLength = value;

					foreach (BillOfLadingNumberCustomisation customisation in BillOfLadingNumberCustomisations)
					{
						customisation.PrefixLength = value;
					}
				}
			}
		}
		int prefixLength = 1;

		public int MaxAllowedLength
		{
			get { return maxAllowedLength; }
			set
			{
				maxAllowedLength = value;
				foreach (BillOfLadingNumberCustomisation customisation in BillOfLadingNumberCustomisations)
				{
					customisation.MaxAllowedLength = value;
				}
			}
		}
		int maxAllowedLength;

		#endregion

		#region Business Object Overrides

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new BillOfLadingNumberCustomisationsByServiceLevel();
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			BillOfLadingNumberCustomisationsByServiceLevel target = (BillOfLadingNumberCustomisationsByServiceLevel)clone;
			target.Categories = Categories;
			target.AllowNonAlphanumericCharacters = AllowNonAlphanumericCharacters;
			target.EnableMacroInsertion = EnableMacroInsertion;
			target.PrefixLength = PrefixLength;
			target.MaxAllowedLength = MaxAllowedLength;
			base.CopyValuesToClone(target);

			foreach (BillOfLadingNumberCustomisation element in BillOfLadingNumberCustomisations)
			{
				BillOfLadingNumberCustomisation targetElement = target.BillOfLadingNumberCustomisations[element.ServiceLevel];

				if (targetElement == null)
				{
					targetElement = target.BillOfLadingNumberCustomisations.AddNew();
					using (targetElement.GetValidationSuspender())
					{
						targetElement.CopyValuesFrom(element);
					}
				}
				else
				{
					using (targetElement.GetValidationSuspender())
					{
						targetElement.CopyValuesFrom(element);
					}
				}
			}

			foreach (BillOfLadingNumberCustomisation element in target.BillOfLadingNumberCustomisations.ToArray<BillOfLadingNumberCustomisation>())
			{
				BillOfLadingNumberCustomisation matchedElement = BillOfLadingNumberCustomisations[element.ServiceLevel];
				if (matchedElement == null)
				{
					target.BillOfLadingNumberCustomisations.Remove(element);
				}
			}
		}

		#endregion

		#region Xml Serialisation

		protected override void WriteServiceLevelCustomisations(System.Xml.XmlWriter writer)
		{
			BillOfLadingNumberCustomisations.WriteXml(writer);
		}

		protected override void ReadServiceLevelCustomisations(System.Xml.XmlReader reader)
		{
			BillOfLadingNumberCustomisations.ReadXml(reader);
		}

		#endregion
	}
}

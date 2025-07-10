using System;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class BillOfLadingNumberCustomisation : AutoBillOfLadingNumberCustomisation, ICanDelete
	{
		public void SetParent(BillOfLadingNumberCustomisationsByServiceLevel parent)
		{
			this.parent = parent;
		}
		BillOfLadingNumberCustomisationsByServiceLevel parent;

		#region Config Properties

		public NumberCustomisationElementCategories Categories
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return categories; }
			set
			{
				if (value != categories)
				{
					categories = value;

					if (elements != null)
					{
						elements.SetCategories(value);
					}
				}
			}
		}
		NumberCustomisationElementCategories categories = NumberCustomisationElementCategories.Default;

		public int PrefixLength
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return prefixLength; }
			set
			{
				if (value != prefixLength)
				{
					prefixLength = value;
					InvalidateMaxGeneratedLength();
				}
			}
		}
		int prefixLength = 1;

		public bool AllowNonAlphanumericCharacters
		{
			get { return allowNonAlphanumericCharacters; }
			set { allowNonAlphanumericCharacters = value; }
		}
		bool allowNonAlphanumericCharacters;

		public bool EnableMacroInsertion
		{
			get { return enableMacroInsertion; }
			set { enableMacroInsertion = value; }
		}
		bool enableMacroInsertion;

		public int MaxAllowedLength { get; set; }

		#endregion

		#region Elements

		public BillOfLadingNumberCustomisationElementView Elements
		{
			get
			{
				if (elements == null)
				{
					elements = new BillOfLadingNumberCustomisationElementView(UnFilteredElements);
					elements.SetCategories(Categories);
					RegisterEditableChildObject(elements);
				}
				return elements;
			}
		}
		BillOfLadingNumberCustomisationElementView elements;

		public BillOfLadingNumberCustomisationElementCollection UnFilteredElements
		{
			get
			{
				if (unfilteredElements == null)
				{
					unfilteredElements = BillOfLadingNumberCustomisationElementCollection.NewAndPopulate(this);
					unfilteredElements.MaxGeneratedLengthChanged += new EventHandler(Elements_MaxGeneratedLengthChanged);
				}
				return unfilteredElements;
			}
		}

		void Elements_MaxGeneratedLengthChanged(object sender, EventArgs e)
		{
			InvalidateMaxGeneratedLength();
		}
		BillOfLadingNumberCustomisationElementCollection unfilteredElements;

		#endregion

		#region BusinessObject Overrides

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();
			CheckDigitAlgorithm = CheckDigitAlgorithmList.Codes.None;
		}

		public override string TableName
		{
			get { return "BillOfLadingNumberCustomisation"; }
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new BillOfLadingNumberCustomisation();
		}

		public void CopyValuesFrom(BillOfLadingNumberCustomisation source)
		{
			Argument.NotNull(source, "source");
			source.CopyValuesToClone(this);
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			BillOfLadingNumberCustomisation target = (BillOfLadingNumberCustomisation)clone;
			target.Categories = Categories;
			target.PrefixLength = PrefixLength;
			target.AllowNonAlphanumericCharacters = AllowNonAlphanumericCharacters;
			target.EnableMacroInsertion = EnableMacroInsertion;
			target.MaxAllowedLength = MaxAllowedLength;

			base.CopyValuesToClone(target);

			foreach (BillOfLadingNumberCustomisationElement element in UnFilteredElements)
			{
				BillOfLadingNumberCustomisationElement targetElement = target.UnFilteredElements[element.Key];

				using (targetElement.GetValidationSuspender())
				{
					targetElement.CopyValuesFrom(element);
				}
			}

			foreach (BillOfLadingNumberCustomisationElement element in target.UnFilteredElements.ToArray<BillOfLadingNumberCustomisationElement>())
			{
				BillOfLadingNumberCustomisationElement matchedElement = UnFilteredElements[element.Key];
				if (matchedElement == null)
				{
					target.UnFilteredElements.Remove(element);
				}
			}
		}

		#endregion

		#region Property Overrides

		public void MakeDefaultServiceLevel()
		{
			base.ServiceLevel = "ALL";
		}

		[List("ServiceLevels")]
		public override ZString ServiceLevel
		{
			get { return base.ServiceLevel; }
			set { base.ServiceLevel = value; }
		}

		public IRefServiceLevel RefServiceLevel
		{
			get { return (IRefServiceLevel)CurrentFactory.LoadTop1(ObjectFactory.GetType<IRefServiceLevel>(), new ZQuery(RefServiceLevelSchema.RS_Code, ServiceLevel)); }
		}

		protected bool ServiceLevel_ReadOnly
		{
			get { return ServiceLevel == "ALL"; }
		}

		public override void ValidateServiceLevel()
		{
			base.ValidateServiceLevel();
			if (parent != null)
			{
				MandatoryValidation.CheckEntered(ServiceLevelInfo);
			}

			if (ServiceLevel != "ALL")
			{
				ListValidation.ErrorIfInvalidCode(ServiceLevelInfo);
			}

			if (!IsUniqueOnParent)
			{
				ServiceLevelInfo.AddError(Res.GetString("7c0376eb-275e-4dd8-81dc-3afe32954fb7", "Service Level is not Unique."));
			}
		}

		ZBool IsUniqueOnParent
		{
			get
			{
				ZBool result = true;
				if (parent != null)
				{
					foreach (BillOfLadingNumberCustomisation customisation in parent.BillOfLadingNumberCustomisations)
					{
						if (customisation != this && customisation.ServiceLevel == ServiceLevel)
						{
							result = false;
							break;
						}
					}
				}
				return result;
			}
		}

		public IRefServiceLevelCollection ServiceLevels
		{
			get { return serviceLevels ?? (serviceLevels = (IRefServiceLevelCollection)Activator.CreateInstance(ObjectFactory.GetType<IRefServiceLevelCollection>(), new object[] { CurrentFactory })); }
		}
		IRefServiceLevelCollection serviceLevels;

		#endregion

		#region Bound Properties

		#region MaxGeneratedLength

		public override ZInt MaxGeneratedLength
		{
			get
			{
				if (maxGeneratedLength == null)
				{
					maxGeneratedLength = CalculateMaxGeneratedLength();
					ValidateMaxGeneratedLength();
				}

				return maxGeneratedLength.Value;
			}
		}

		void InvalidateMaxGeneratedLength()
		{
			maxGeneratedLength = null;
			MaxGeneratedLengthInfo.RefreshBinding();
		}

		int? maxGeneratedLength;

		public override void ValidateMaxGeneratedLength()
		{
			base.ValidateMaxGeneratedLength();
			if (MaxAllowedLength != 0 && maxGeneratedLength > MaxAllowedLength)
			{
				MaxGeneratedLengthInfo.AddError(Res.GetString("ac05a508-b076-43e9-ae11-38d76f759574", "Value exceeds maximum length"));
			}
		}

		#endregion

		#region Remove Fountain Prefix

		public override ZBool RemoveFountainPrefix
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.RemoveFountainPrefix; }
			set
			{
				base.RemoveFountainPrefix = value;
				InvalidateMaxGeneratedLength();
			}
		}

		#endregion

		#region Auto Allocate Master Bill Numbers To Consols

		public override ZBool AutoAllocateMasterBillNumbersToConsols
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.AutoAllocateMasterBillNumbersToConsols; }
			set
			{
				base.AutoAllocateMasterBillNumbersToConsols = value;
				InvalidateMaxGeneratedLength();
			}
		}

		#endregion

		#region CheckDigitAlgorithm

		[List("CheckDigitAlgorithm_List")]
		public override ZString CheckDigitAlgorithm
		{
			get { return base.CheckDigitAlgorithm; }
			set
			{
				base.CheckDigitAlgorithm = value;
				InvalidateMaxGeneratedLength();
			}
		}

		public override void ValidateCheckDigitAlgorithm()
		{
			base.ValidateCheckDigitAlgorithm();
			ListValidation.ErrorIfInvalidCode(CheckDigitAlgorithmInfo, CheckDigitAlgorithm_List);

			if (!CheckDigitAlgorithmInfo.HasErrors()
					&& CheckDigitAlgorithm != CheckDigitAlgorithmList.Codes.None
					&& !Elements.HasIncludedElementWithCheckDigit)
			{
				CheckDigitAlgorithmInfo.AddError(Res.GetString("9192604b-6588-4a0e-8461-89484737582c", "The Check Digit Algorithm is {0}. At least one element needs to be included in the check digit algorithm.", CheckDigitAlgorithm));
			}
		}

		public CodeDescriptionPairList CheckDigitAlgorithm_List
		{
			get
			{
				CodeDescriptionPairList result = new CheckDigitAlgorithmList();
				result.DefaultCode = CheckDigitAlgorithmList.Codes.None;
				return result;
			}
		}

		#endregion

		#endregion

		#region Implementation

		ZInt CalculateMaxGeneratedLength()
		{
			int result = UnFilteredElements.CalcMaxGeneratedLength();

			if (CheckDigitAlgorithm != CheckDigitAlgorithmList.Codes.None)
			{
				result++;
			}

			if (!RemoveFountainPrefix)
			{
				result += PrefixLength;
			}

			return result;
		}

		public override bool Equals(object obj)
		{
			var other = obj as BillOfLadingNumberCustomisation;
			return other != null &&
				other.ServiceLevel == ServiceLevel &&
				other.AllowNonAlphanumericCharacters == AllowNonAlphanumericCharacters &&
				other.EnableMacroInsertion == EnableMacroInsertion &&
				other.Categories == Categories &&
				other.PrefixLength == PrefixLength &&
				other.CheckDigitAlgorithm == CheckDigitAlgorithm &&
				other.Elements.ContainsSameElementsInAnyOrder(Elements) &&
				other.RemoveFountainPrefix == RemoveFountainPrefix &&
				other.AutoAllocateMasterBillNumbersToConsols == AutoAllocateMasterBillNumbersToConsols &&
				other.UseShipmentSequenceNumber == UseShipmentSequenceNumber;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Decruftification", "WTG3007:Overrides should not simply call base.", Justification = "Required because Equals is overridden")]
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
		#endregion

		#region Read / Write Number Elements

		protected override void WriteNumberElements(XmlWriter writer)
		{
			UnFilteredElements.WriteXml(writer);
		}

		protected override void ReadNumberElements(XmlReader reader)
		{
			UnFilteredElements.ReadXml(reader);
		}

		#endregion

		#region ICanDelete Members

		bool ICanDelete.CanDelete
		{
			get { return ServiceLevel != "ALL" || !IsUniqueOnParent; }
		}

		MultilingualString ICanDelete.ReasonForNotAbleToDelete
		{
			get { return ResString.GetMultilingualString("9418cfe2-696b-4be2-a3e6-bfdf31239f48", "Cannot delete the Default Fallback 'ALL'"); }
		}

		#endregion
	}
}

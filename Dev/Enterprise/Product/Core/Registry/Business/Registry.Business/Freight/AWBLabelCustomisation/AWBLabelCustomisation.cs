using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class AWBLabelCustomisation : RegistryBusinessObjectTemplate
	{
		#region Schema

		public static class Schema
		{
			public const string CustomDesign = "CustomDesign";

			public const string OptionalInformation1 = "OptionalInformation1";
			public const string OptionalInformation2 = "OptionalInformation2";
			public const string OptionalInformation3 = "OptionalInformation3";
			public const string OptionalInformation4 = "OptionalInformation4";
			public const string OptionalInformation5 = "OptionalInformation5";
			public const string OptionalInformation6 = "OptionalInformation6";

			public const string OptionalDescription1 = "OptionalDescription1";
			public const string OptionalDescription2 = "OptionalDescription2";
			public const string OptionalDescription3 = "OptionalDescription3";
			public const string OptionalDescription4 = "OptionalDescription4";
			public const string OptionalDescription5 = "OptionalDescription5";
			public const string OptionalDescription6 = "OptionalDescription6";

			public const string BarcodeConsolOrigin = "BarcodeConsolOrigin";
			public const string BarcodeConsolDestination = "BarcodeConsolDestination";
			public const string BarcodeConsolPieceCount = "BarcodeConsolPieceCount";
			public const string BarcodeConsolWeight = "BarcodeConsolWeight";

			public const string BarcodeHousebillNumber = "BarcodeHousebillNumber";
			public const string BarcodeShipmentPieceNumber = "BarcodeShipmentPieceNumber";
			public const string BarcodeShipmentPieceCount = "BarcodeShipmentPieceCount";
			public const string BarcodeShipmentWeight = "BarcodeShipmentWeight";
			public const string BarcodeShipmentHandlingInformation = "BarcodeShipmentHandlingInformation";
		}

		#endregion

		#region Properties

		#region Optional Information Design

		[List("CustomDesignList")]
		public ZString CustomDesign
		{
			get { return fCustomDesign; }
			set
			{
				SetNonPersistentPropertyValue<ZString>(CustomDesignInfo, ref fCustomDesign, value);
				if (!IsValidationSuspended)
				{
					ValidateCustomDesign();
				}
			}
		}
		ZString fCustomDesign;

		public ZPropertyInfo CustomDesignInfo
		{
			get { return GetZPropertyInfo(Schema.CustomDesign); }
		}

		public void ValidateCustomDesign()
		{
			CustomDesignInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(CustomDesignInfo, CustomDesignList);
		}

		#endregion

		#region Optional Information

		[List("OptionalInformationList")]
		public ZString OptionalInformation1
		{
			get { return fOptionalInformation1; }
			set
			{
				SetNonPersistentPropertyValue<ZString>(OptionalInformation1Info, ref fOptionalInformation1, value);
				if (!IsValidationSuspended)
				{
					ValidateOptionalInformation1();
				}
				OptionalDescription1 = value.ToUpper();
			}
		}
		ZString fOptionalInformation1;

		public ZPropertyInfo OptionalInformation1Info
		{
			get { return GetZPropertyInfo(Schema.OptionalInformation1); }
		}

		public int OptionalInformation1_MaxLength
		{
			get { return OptionalInformationMaximumLength; }
		}

		public void ValidateOptionalInformation1()
		{
			OptionalInformation1Info.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(OptionalInformation1Info, OptionalInformationList);
		}

		[List("OptionalInformationList")]
		public ZString OptionalInformation2
		{
			get { return fOptionalInformation2; }
			set
			{
				SetNonPersistentPropertyValue<ZString>(OptionalInformation2Info, ref fOptionalInformation2, value);
				if (!IsValidationSuspended)
				{
					ValidateOptionalInformation2();
				}
				OptionalDescription2 = value.ToUpper();
			}
		}
		ZString fOptionalInformation2;

		public ZPropertyInfo OptionalInformation2Info
		{
			get { return GetZPropertyInfo(Schema.OptionalInformation2); }
		}

		public int OptionalInformation2_MaxLength
		{
			get { return OptionalInformationMaximumLength; }
		}

		public void ValidateOptionalInformation2()
		{
			OptionalInformation2Info.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(OptionalInformation2Info, OptionalInformationList);
		}

		[List("OptionalInformationList")]
		public ZString OptionalInformation3
		{
			get { return fOptionalInformation3; }
			set
			{
				SetNonPersistentPropertyValue<ZString>(OptionalInformation3Info, ref fOptionalInformation3, value);
				if (!IsValidationSuspended)
				{
					ValidateOptionalInformation3();
				}
				OptionalDescription3 = value.ToUpper();
			}
		}
		ZString fOptionalInformation3;

		public ZPropertyInfo OptionalInformation3Info
		{
			get { return GetZPropertyInfo(Schema.OptionalInformation3); }
		}

		public int OptionalInformation3_MaxLength
		{
			get { return OptionalInformationMaximumLength; }
		}

		public void ValidateOptionalInformation3()
		{
			OptionalInformation3Info.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(OptionalInformation3Info, OptionalInformationList);
		}

		[List("OptionalInformationList")]
		public ZString OptionalInformation4
		{
			get { return fOptionalInformation4; }
			set
			{
				SetNonPersistentPropertyValue<ZString>(OptionalInformation4Info, ref fOptionalInformation4, value);
				if (!IsValidationSuspended)
				{
					ValidateOptionalInformation4();
				}
				OptionalDescription4 = value.ToUpper();
			}
		}
		ZString fOptionalInformation4;

		public ZPropertyInfo OptionalInformation4Info
		{
			get { return GetZPropertyInfo(Schema.OptionalInformation4); }
		}

		public int OptionalInformation4_MaxLength
		{
			get { return OptionalInformationMaximumLength; }
		}

		public void ValidateOptionalInformation4()
		{
			OptionalInformation4Info.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(OptionalInformation4Info, OptionalInformationList);
		}

		[List("OptionalInformationList")]
		public ZString OptionalInformation5
		{
			get { return fOptionalInformation5; }
			set
			{
				SetNonPersistentPropertyValue<ZString>(OptionalInformation5Info, ref fOptionalInformation5, value);
				if (!IsValidationSuspended)
				{
					ValidateOptionalInformation5();
				}
				OptionalDescription5 = value.ToUpper();
			}
		}
		ZString fOptionalInformation5;

		public ZPropertyInfo OptionalInformation5Info
		{
			get { return GetZPropertyInfo(Schema.OptionalInformation5); }
		}

		public int OptionalInformation5_MaxLength
		{
			get { return OptionalInformationMaximumLength; }
		}

		public void ValidateOptionalInformation5()
		{
			OptionalInformation5Info.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(OptionalInformation5Info, OptionalInformationList);
		}

		[List("OptionalInformationList")]
		public ZString OptionalInformation6
		{
			get { return fOptionalInformation6; }
			set
			{
				SetNonPersistentPropertyValue<ZString>(OptionalInformation6Info, ref fOptionalInformation6, value);
				if (!IsValidationSuspended)
				{
					ValidateOptionalInformation6();
				}
				OptionalDescription6 = value.ToUpper();
			}
		}
		ZString fOptionalInformation6;

		public ZPropertyInfo OptionalInformation6Info
		{
			get { return GetZPropertyInfo(Schema.OptionalInformation6); }
		}

		public int OptionalInformation6_MaxLength
		{
			get { return OptionalInformationMaximumLength; }
		}

		public void ValidateOptionalInformation6()
		{
			OptionalInformation6Info.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(OptionalInformation6Info, OptionalInformationList);
		}

		#endregion

		#region Optional Description

		public ZString OptionalDescription1
		{
			get { return fOptionalDescription1; }
			set
			{
				SetNonPersistentPropertyValue<ZString>(OptionalDescription1Info, ref fOptionalDescription1, value);
			}
		}
		ZString fOptionalDescription1;

		public ZPropertyInfo OptionalDescription1Info
		{
			get { return GetZPropertyInfo(Schema.OptionalDescription1); }
		}

		public int OptionalDescription1_MaxLength
		{
			get { return OptionalDescriptionMaximumLength; }
		}

		public ZString OptionalDescription2
		{
			get { return fOptionalDescription2; }
			set
			{
				SetNonPersistentPropertyValue<ZString>(OptionalDescription2Info, ref fOptionalDescription2, value);
			}
		}
		ZString fOptionalDescription2;

		public ZPropertyInfo OptionalDescription2Info
		{
			get { return GetZPropertyInfo(Schema.OptionalDescription2); }
		}

		public int OptionalDescription2_MaxLength
		{
			get { return OptionalDescriptionMaximumLength; }
		}

		public ZString OptionalDescription3
		{
			get { return fOptionalDescription3; }
			set
			{
				SetNonPersistentPropertyValue<ZString>(OptionalDescription3Info, ref fOptionalDescription3, value);
			}
		}
		ZString fOptionalDescription3;

		public ZPropertyInfo OptionalDescription3Info
		{
			get { return GetZPropertyInfo(Schema.OptionalDescription3); }
		}

		public int OptionalDescription3_MaxLength
		{
			get { return OptionalDescriptionMaximumLength; }
		}

		public ZString OptionalDescription4
		{
			get { return fOptionalDescription4; }
			set
			{
				SetNonPersistentPropertyValue<ZString>(OptionalDescription4Info, ref fOptionalDescription4, value);
			}
		}
		ZString fOptionalDescription4;

		public ZPropertyInfo OptionalDescription4Info
		{
			get { return GetZPropertyInfo(Schema.OptionalDescription4); }
		}

		public int OptionalDescription4_MaxLength
		{
			get { return OptionalDescriptionMaximumLength; }
		}

		public ZString OptionalDescription5
		{
			get { return fOptionalDescription5; }
			set
			{
				SetNonPersistentPropertyValue<ZString>(OptionalDescription5Info, ref fOptionalDescription5, value);
			}
		}
		ZString fOptionalDescription5;

		public ZPropertyInfo OptionalDescription5Info
		{
			get { return GetZPropertyInfo(Schema.OptionalDescription5); }
		}

		public int OptionalDescription5_MaxLength
		{
			get { return OptionalDescriptionMaximumLength; }
		}

		public ZString OptionalDescription6
		{
			get { return fOptionalDescription6; }
			set
			{
				SetNonPersistentPropertyValue<ZString>(OptionalDescription6Info, ref fOptionalDescription6, value);
			}
		}
		ZString fOptionalDescription6;

		public ZPropertyInfo OptionalDescription6Info
		{
			get { return GetZPropertyInfo(Schema.OptionalDescription6); }
		}

		public int OptionalDescription6_MaxLength
		{
			get { return OptionalDescriptionMaximumLength; }
		}

		#endregion

		#region Barcode

		public ZBool BarcodeConsolOrigin
		{
			get { return fBarcodeConsolOrigin; }
			set
			{
				SetNonPersistentPropertyValue<ZBool>(BarcodeConsolOriginInfo, ref fBarcodeConsolOrigin, value);
			}
		}
		ZBool fBarcodeConsolOrigin;

		public ZPropertyInfo BarcodeConsolOriginInfo
		{
			get { return GetZPropertyInfo(Schema.BarcodeConsolOrigin); }
		}

		public ZBool BarcodeConsolDestination
		{
			get { return fBarcodeConsolDestination; }
			set
			{
				SetNonPersistentPropertyValue<ZBool>(BarcodeConsolDestinationInfo, ref fBarcodeConsolDestination, value);
			}
		}
		ZBool fBarcodeConsolDestination;

		public ZPropertyInfo BarcodeConsolDestinationInfo
		{
			get { return GetZPropertyInfo(Schema.BarcodeConsolDestination); }
		}

		public ZBool BarcodeConsolPieceCount
		{
			get { return fBarcodeConsolPieceCount; }
			set
			{
				SetNonPersistentPropertyValue<ZBool>(BarcodeConsolPieceCountInfo, ref fBarcodeConsolPieceCount, value);
			}
		}
		ZBool fBarcodeConsolPieceCount;

		public ZPropertyInfo BarcodeConsolPieceCountInfo
		{
			get { return GetZPropertyInfo(Schema.BarcodeConsolPieceCount); }
		}

		public ZBool BarcodeConsolWeight
		{
			get { return fBarcodeConsolWeight; }
			set
			{
				SetNonPersistentPropertyValue<ZBool>(BarcodeConsolWeightInfo, ref fBarcodeConsolWeight, value);
			}
		}
		ZBool fBarcodeConsolWeight;

		public ZPropertyInfo BarcodeConsolWeightInfo
		{
			get { return GetZPropertyInfo(Schema.BarcodeConsolWeight); }
		}

		public ZBool BarcodeHousebillNumber
		{
			get { return fBarcodeHousebillNumber; }
			set
			{
				SetNonPersistentPropertyValue<ZBool>(BarcodeHousebillNumberInfo, ref fBarcodeHousebillNumber, value);
			}
		}
		ZBool fBarcodeHousebillNumber;

		public ZPropertyInfo BarcodeHousebillNumberInfo
		{
			get { return GetZPropertyInfo(Schema.BarcodeHousebillNumber); }
		}

		public ZBool BarcodeShipmentPieceNumber
		{
			get { return fBarcodeShipmentPieceNumber; }
			set
			{
				SetNonPersistentPropertyValue<ZBool>(BarcodeShipmentPieceNumberInfo, ref fBarcodeShipmentPieceNumber, value);
			}
		}
		ZBool fBarcodeShipmentPieceNumber;

		public ZPropertyInfo BarcodeShipmentPieceNumberInfo
		{
			get { return GetZPropertyInfo(Schema.BarcodeShipmentPieceNumber); }
		}

		public ZBool BarcodeShipmentPieceCount
		{
			get { return fBarcodeShipmentPieceCount; }
			set
			{
				SetNonPersistentPropertyValue<ZBool>(BarcodeShipmentPieceCountInfo, ref fBarcodeShipmentPieceCount, value);
			}
		}
		ZBool fBarcodeShipmentPieceCount;

		public ZPropertyInfo BarcodeShipmentPieceCountInfo
		{
			get { return GetZPropertyInfo(Schema.BarcodeShipmentPieceCount); }
		}

		public ZBool BarcodeShipmentWeight
		{
			get { return fBarcodeShipmentWeight; }
			set
			{
				SetNonPersistentPropertyValue<ZBool>(BarcodeShipmentWeightInfo, ref fBarcodeShipmentWeight, value);
			}
		}
		ZBool fBarcodeShipmentWeight;

		public ZPropertyInfo BarcodeShipmentWeightInfo
		{
			get { return GetZPropertyInfo(Schema.BarcodeShipmentWeight); }
		}

		public ZBool BarcodeShipmentHandlingInformation
		{
			get { return fBarcodeShipmentHandlingInformation; }
			set
			{
				SetNonPersistentPropertyValue<ZBool>(BarcodeShipmentHandlingInformationInfo, ref fBarcodeShipmentHandlingInformation, value);
			}
		}
		ZBool fBarcodeShipmentHandlingInformation;

		public ZPropertyInfo BarcodeShipmentHandlingInformationInfo
		{
			get { return GetZPropertyInfo(Schema.BarcodeShipmentHandlingInformation); }
		}

		#endregion

		#region Implementation

		ZInt OptionalInformationMaximumLength
		{
			get
			{
				if (fOptionalInformationMaximumLength == 0)
				{
					foreach (CodeDescriptionPair pair in OptionalInformationList)
					{
						if (pair.Code.Length > fOptionalInformationMaximumLength)
						{
							fOptionalInformationMaximumLength = pair.Code.Length;
						}
					}
				}

				return fOptionalInformationMaximumLength;
			}
		}
		ZInt fOptionalInformationMaximumLength;

		ZInt OptionalDescriptionMaximumLength
		{
			get { return OptionalInformationMaximumLength; }
		}

		public int CustomDesign_MaxLength
		{
			get
			{
				if (fCustomDesignMaximumLength == 0)
				{
					foreach (CodeDescriptionPair pair in CustomDesignList)
					{
						if (pair.Code.Length > fCustomDesignMaximumLength)
						{
							fCustomDesignMaximumLength = pair.Code.Length;
						}
					}
				}

				return fCustomDesignMaximumLength;
			}
		}
		ZInt fCustomDesignMaximumLength;

		#endregion

		#endregion

		#region Lookups

		public CodeDescriptionPairList OptionalInformationList
		{
			get { return fOptionalInformationList ?? (fOptionalInformationList = new AWBLabelOptionalInformationList()); }
		}
		CodeDescriptionPairList fOptionalInformationList;

		public CodeDescriptionPairList CustomDesignList
		{
			get { return fCustomDesignList ?? (fCustomDesignList = new AWBLabelCustomDesignList()); }
		}
		CodeDescriptionPairList fCustomDesignList;

		#endregion

		#region Overrides

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new AWBLabelCustomisation();
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateCustomDesign();
			ValidateOptionalInformation1();
			ValidateOptionalInformation2();
			ValidateOptionalInformation3();
			ValidateOptionalInformation4();
			ValidateOptionalInformation5();
			ValidateOptionalInformation6();
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.CustomDesign, CustomDesign);

			writer.WriteElementString(Schema.OptionalInformation1, OptionalInformation1);
			writer.WriteElementString(Schema.OptionalInformation2, OptionalInformation2);
			writer.WriteElementString(Schema.OptionalInformation3, OptionalInformation3);
			writer.WriteElementString(Schema.OptionalInformation4, OptionalInformation4);
			writer.WriteElementString(Schema.OptionalInformation5, OptionalInformation5);
			writer.WriteElementString(Schema.OptionalInformation6, OptionalInformation6);

			writer.WriteElementString(Schema.OptionalDescription1, OptionalDescription1);
			writer.WriteElementString(Schema.OptionalDescription2, OptionalDescription2);
			writer.WriteElementString(Schema.OptionalDescription3, OptionalDescription3);
			writer.WriteElementString(Schema.OptionalDescription4, OptionalDescription4);
			writer.WriteElementString(Schema.OptionalDescription5, OptionalDescription5);
			writer.WriteElementString(Schema.OptionalDescription6, OptionalDescription6);

			writer.WriteElementString(Schema.BarcodeConsolOrigin, BarcodeConsolOrigin.ToString());
			writer.WriteElementString(Schema.BarcodeConsolDestination, BarcodeConsolDestination.ToString());
			writer.WriteElementString(Schema.BarcodeConsolPieceCount, BarcodeConsolPieceCount.ToString());
			writer.WriteElementString(Schema.BarcodeConsolWeight, BarcodeConsolWeight.ToString());
			writer.WriteElementString(Schema.BarcodeHousebillNumber, BarcodeHousebillNumber.ToString());
			writer.WriteElementString(Schema.BarcodeShipmentPieceNumber, BarcodeShipmentPieceNumber.ToString());
			writer.WriteElementString(Schema.BarcodeShipmentPieceCount, BarcodeShipmentPieceCount.ToString());
			writer.WriteElementString(Schema.BarcodeShipmentWeight, BarcodeShipmentWeight.ToString());
			writer.WriteElementString(Schema.BarcodeShipmentHandlingInformation, BarcodeShipmentHandlingInformation.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			CustomDesign = reader.ReadElementString(Schema.CustomDesign);

			OptionalInformation1 = reader.ReadElementString(Schema.OptionalInformation1);
			OptionalInformation2 = reader.ReadElementString(Schema.OptionalInformation2);
			OptionalInformation3 = reader.ReadElementString(Schema.OptionalInformation3);
			OptionalInformation4 = reader.ReadElementString(Schema.OptionalInformation4);
			OptionalInformation5 = reader.ReadElementString(Schema.OptionalInformation5);
			OptionalInformation6 = reader.ReadElementString(Schema.OptionalInformation6);

			OptionalDescription1 = reader.ReadElementString(Schema.OptionalDescription1);
			OptionalDescription2 = reader.ReadElementString(Schema.OptionalDescription2);
			OptionalDescription3 = reader.ReadElementString(Schema.OptionalDescription3);
			OptionalDescription4 = reader.ReadElementString(Schema.OptionalDescription4);
			OptionalDescription5 = reader.ReadElementString(Schema.OptionalDescription5);
			OptionalDescription6 = reader.ReadElementString(Schema.OptionalDescription6);

			BarcodeConsolOrigin = new ZBool(reader.ReadElementString(Schema.BarcodeConsolOrigin));
			BarcodeConsolDestination = new ZBool(reader.ReadElementString(Schema.BarcodeConsolDestination));
			BarcodeConsolPieceCount = new ZBool(reader.ReadElementString(Schema.BarcodeConsolPieceCount));
			BarcodeConsolWeight = new ZBool(reader.ReadElementString(Schema.BarcodeConsolWeight));
			BarcodeHousebillNumber = new ZBool(reader.ReadElementString(Schema.BarcodeHousebillNumber));
			BarcodeShipmentPieceNumber = new ZBool(reader.ReadElementString(Schema.BarcodeShipmentPieceNumber));
			BarcodeShipmentPieceCount = new ZBool(reader.ReadElementString(Schema.BarcodeShipmentPieceCount));
			BarcodeShipmentWeight = new ZBool(reader.ReadElementString(Schema.BarcodeShipmentWeight));
			BarcodeShipmentHandlingInformation = new ZBool(reader.ReadElementString(Schema.BarcodeShipmentHandlingInformation));
		}

		#endregion

		public static AWBLabelCustomisation GetDefault()
		{
			AWBLabelCustomisation awbLabel = new AWBLabelCustomisation();
			awbLabel.CustomDesign = AWBLabelCustomDesignList.Codes.Default;

			awbLabel.OptionalInformation1 = AWBLabelOptionalInformationList.Codes.HousebillNumber;
			awbLabel.OptionalInformation2 = AWBLabelOptionalInformationList.Codes.ShipmentPieceNumberOfCount;
			awbLabel.OptionalInformation3 = AWBLabelOptionalInformationList.Codes.ConsolOrigin;
			awbLabel.OptionalInformation4 = AWBLabelOptionalInformationList.Codes.Blank;
			awbLabel.OptionalInformation5 = AWBLabelOptionalInformationList.Codes.Blank;
			awbLabel.OptionalInformation6 = AWBLabelOptionalInformationList.Codes.Blank;

			awbLabel.OptionalDescription1 = AWBLabelOptionalInformationList.Codes.HAWBNo;
			awbLabel.OptionalDescription2 = AWBLabelOptionalInformationList.Codes.PieceCount;
			awbLabel.OptionalDescription3 = AWBLabelOptionalInformationList.Codes.Origin;
			awbLabel.OptionalDescription4 = AWBLabelOptionalInformationList.Codes.Blank;
			awbLabel.OptionalDescription5 = AWBLabelOptionalInformationList.Codes.Blank;
			awbLabel.OptionalDescription6 = AWBLabelOptionalInformationList.Codes.Blank;

			awbLabel.BarcodeConsolOrigin = false;
			awbLabel.BarcodeConsolDestination = false;
			awbLabel.BarcodeConsolPieceCount = false;
			awbLabel.BarcodeConsolWeight = false;
			awbLabel.BarcodeHousebillNumber = true;
			awbLabel.BarcodeShipmentPieceNumber = false;
			awbLabel.BarcodeShipmentPieceCount = false;
			awbLabel.BarcodeShipmentWeight = false;
			awbLabel.BarcodeShipmentHandlingInformation = false;

			return awbLabel;
		}
	}
}

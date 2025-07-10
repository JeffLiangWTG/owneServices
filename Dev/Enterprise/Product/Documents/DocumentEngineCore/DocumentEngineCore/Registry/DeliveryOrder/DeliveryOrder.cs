using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngineCore.Registry
{
	[XmlSerializerAssembly("Enterprise.DocumentEngineCore.XmlSerializers")]
	public class DeliveryOrder : AutoDeliveryOrder
	{
		public DeliveryOrder() { }

		public DeliveryOrder(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory) { }

		#region Schema and Constants

		public static class PrintConstants
		{
			public static class Code
			{
				public const string PDO = "PDO";
				public const string PCT = "PCT";
			}

			public static class Description
			{
				public static string PDO
				{
					get { return Res.GetString("5718243e-4389-4796-80b7-abc74424f3be", "Once per Delivery order"); }
				}
				public static string PCT
				{
					get { return Res.GetString("474cae46-5459-46b8-9230-ebfcc111a6fe", "Once per all the containers on a bill"); }
				}
			}
		}

		#endregion

		#region Properties

		public override Image Image
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.Image; }
			set
			{
				base.Image = value;
				imageSize = null;
			}
		}

		public long ImageSize
		{
			get
			{
				if (!imageSize.HasValue)
				{
					imageSize = GetImageSize(Image);
				}
				return imageSize.Value;
			}
		}
		long? imageSize;

		#endregion

		#region Overrides

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new DeliveryOrder(fallbackLevel, factory);
		}

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("57b62956-d4a4-435b-8aec-6e5dda16f475", "Delivery Order"); }
		}

		#endregion

		#region Validation

		protected override void CheckImage()
		{
			base.CheckImage();

			if (Image == null)
			{
				AddRowError(Res.GetString("ceed91ca-17a8-485e-90a5-61e95479876d", "Please select an Image."));
			}
			else if (ImageSize > 5000000)
			{
				AddRowError(Res.GetString("2409421f-3948-4e71-9d31-91613ec7f16a", "This image is {0}, this is way to big. you should be able to get a full page image of text down to around 500KB.", FormatFileSize(ImageSize)));
			}
			else if (ImageSize > 1000000)
			{
				AddRowWarning(Res.GetString("8657decf-252d-4f84-8998-de8389a44330", "This image is {0}, this is a bit excessive. you should be able to get a full page image of text down to around 500KB.", FormatFileSize(ImageSize)));
			}
		}

		public override void ValidatePrincipalPK()
		{
			base.ValidatePrincipalPK();

			MandatoryValidation.CheckEntered(PrincipalPKInfo, Res.GetString("63aab242-c4b4-46a6-bb48-6d3ee98133d0", "Principal"));
			ListValidation.ErrorIfInvalidPK(PrincipalPKInfo, Principals, ResString.GetMultilingualString("0a5efd6c-81f1-4afc-8679-7cfb2e6df7d3", "Enter a valid principal."));

			if (ParentCollections.Count > 0)
			{
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(PrincipalPKInfo, Res.GetString("008401a3-03e5-4dea-b9f9-b363007575eb", "A principal may only appear once in this list."));
			}
		}

		public override void ValidatePrintParameter()
		{
			base.ValidatePrintParameter();

			MandatoryValidation.CheckEntered(PrintParameterInfo);

			if (!PrintParameters.ContainsCode(PrintParameter))
			{
				PrintParameterInfo.AddError(Res.GetString("12c7d85d-2d56-4493-93be-2e86a3f86b0d", "Enter a valid print parameter."));
			}
		}

		#endregion

		#region Principals

		public BusinessObjectCollection Principals
		{
			get
			{
				if (principals == null)
				{
					principals = (BusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<MasterFiles.Integration.IShipsAgencyPrincipalCollection>(), CurrentFactory);
				}

				return principals;
			}
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		BusinessObjectCollection principals;

		#endregion

		#region PrintParameters

		public CodeDescriptionPairList PrintParameters
		{
			get
			{
				if (printParameters == null)
				{
					printParameters = new CodeDescriptionPairList();
					printParameters.AddPair(DeliveryOrder.PrintConstants.Code.PDO, DeliveryOrder.PrintConstants.Description.PDO);
					printParameters.AddPair(DeliveryOrder.PrintConstants.Code.PCT, DeliveryOrder.PrintConstants.Description.PCT);
				}

				return printParameters;
			}
		}
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		CodeDescriptionPairList printParameters;

		#endregion

		#region Implementation

		long GetImageSize(Image image)
		{
			if (image == null)
			{
				return 0;
			}
			else
			{
				using (MemoryStream stream = new MemoryStream())
				{
					image.Save(stream, ImageFormat.Png);
					return stream.Length;
				}
			}
		}

		string FormatFileSize(long sizeInBytes)
		{
			int factor = 0;
			double size = sizeInBytes;

			while (size > 1024 && factor < 2)
			{
				size = Math.Round(size / 1024, 3, MidpointRounding.AwayFromZero);
				factor++;
			}

			string suffix;

			switch (factor)
			{
				case 0: suffix = "B"; break;
				case 1: suffix = "KB"; break;
				case 2: suffix = "MB"; break;
				default: throw new InvalidOperationException();
			}

			return size.ToString() + suffix;
		}

		#endregion
	}
}

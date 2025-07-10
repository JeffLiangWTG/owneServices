namespace Enterprise.ZArchitecture.Environment
{
	using System;
	using CargoWise.Data;
	using Enterprise.Core;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Schema;
	using WTG.StaticAnalysis.Annotation;
	using ResString = Core.SourceGenerated.ResString;

	public static class RegistryConstants
	{
		public static class Packing
		{
			// these DataContexts print multiple labels, we don't want these auto-printed
			public static readonly Constants.DataContext[] NonAutoPrintingDataContexts = new[]
			{
				Constants.DataContext.GenericBasicLabelAll,
				Constants.DataContext.GenericDeliveryLabelAll,
				Constants.DataContext.GenericProductDeliveryLabelAll,
				Constants.DataContext.GenericProductLabelAll,
				Constants.DataContext.GenericRetailersLabelAll,
				Constants.DataContext.GenericCarrierLabelAll,
				Constants.DataContext.GenericDeliveryIDLabelAll,
				Constants.DataContext.GenericAuditVarianceLabelAll,
			};
		}

		public static MultilingualString GetCategory(params MultilingualString[] path)
		{
			return MultilingualString.Join(RegistryItemSet.Delimiter, path);
		}

		public static MultilingualString GetPathForCategory(MultilingualString category)
		{
			return GetCategory(ResString.GetMultilingualString("7608da29-15f3-4093-b3c2-ded8c9b8a4bf", "Registry"), category).Replace(RegistryItemSet.Delimiter, " > ");
		}

		public static class DepartmentPKs
		{
			public static readonly Guid GatewayDomesticAir = new Guid("512A356C-AB93-463A-96D2-EA020654ED3F");      // GDA
			public static readonly Guid GatewayImportAir = new Guid("566CE3DD-925B-424B-9BB3-7FBA564AE668");        // GIA
			public static readonly Guid GatewayExportAir = new Guid("72DFB7C1-6686-45A4-A20F-6A0F01214B0B");        // GEA
			public static readonly Guid GatewayImportSea = new Guid("12A82868-0011-4C35-86A4-67CA542353F3");        // GIS
			public static readonly Guid GatewayDomesticSea = new Guid("1EAA90CE-C117-4C54-A329-A61ECBC8684C");      // GDS
			public static readonly Guid GatewayExportSea = new Guid("9CBFBDAF-42CE-49B9-9D07-97224A62295F");        // GES
			public static readonly Guid GatewayExportRoad = new Guid("13F284E0-0DDD-4636-83D5-32C01F48FBAC");       // GER
			public static readonly Guid GatewayImportRoad = new Guid("4286A084-B596-4990-BCDE-D2F17362880C");       // GIR
			public static readonly Guid GatewayDomesticRoad = new Guid("5D17773D-FE78-4FAC-90DC-D7AED9A9B84B");     // GDR
			public static readonly Guid GatewayImportRail = new Guid("18B9C0D7-EDA4-4E71-93E3-1F3049BB5884");       // GIL
			public static readonly Guid GatewayDomesticRail = new Guid("9809ACCC-9BDF-4EC3-8EA1-B185A307F2A4");     // GDL
			public static readonly Guid GatewayExportRail = new Guid("F657F19E-3821-4437-BE58-1E266C4412DF");       // GEL

			public static readonly Guid ForwardingExportAir = new Guid("F5C72696-19AD-4759-879F-89C8532FF238");    // FEA
			public static readonly Guid ForwardingExportRail = new Guid("7A61D193-3B37-40DE-A915-F69C78464E24");   // FEL
			public static readonly Guid ForwardingExportRoad = new Guid("1C782850-1DD2-4C1B-9786-45755F897407");   // FER
			public static readonly Guid ForwardingExportSea = new Guid("57F778C1-DAF6-46E0-B7DC-AF01C161C936");    // FES
			public static readonly Guid ForwardingImportAir = new Guid("8C3B5BA4-4012-406A-9810-7BD6A746E6B6");    // FIA
			public static readonly Guid ForwardingImportRail = new Guid("6039945E-EDE5-40CC-9106-4F4A8E217759");   // FIL
			public static readonly Guid ForwardingImportRoad = new Guid("2B67864D-42E9-4A43-A9C8-09D2083C4227");   // FIR
			public static readonly Guid ForwardingImportSea = new Guid("AB9930EB-0BF7-4C87-ADE1-6985B0226970");    // FIS
			public static readonly Guid ForwardingDomesticAir = new Guid("98241427-BEA2-4F31-B0C9-4566DEA3811E");  // FDA
			public static readonly Guid ForwardingDomesticRail = new Guid("583890C5-CC83-4A43-8F5A-16D3FD6295CD"); // FDL
			public static readonly Guid ForwardingDomesticRoad = new Guid("B824E3DC-EACE-4A17-B868-1AF5724E3C56"); // FDR
			public static readonly Guid ForwardingDomesticSea = new Guid("237D028B-1864-4467-B34B-0AAC3F990C03");  // FDS
			public static readonly Guid MasterAwb = new Guid("F5C72696-19AD-4759-879F-89C8532FF238");              // FEA

			public static readonly Guid ClearanceExportAir = new Guid("1E2C1BC2-1E57-4312-8A70-05B7B840B053");     // CEA
			public static readonly Guid ClearanceExportRail = new Guid("7210286B-37BD-4054-A27E-0FA922D6EE0A");    // CEL
			public static readonly Guid ClearanceExportRoad = new Guid("8C334817-BB6B-4EDB-B9B4-A57A17B23E13");    // CER
			public static readonly Guid ClearanceExportSea = new Guid("570C4273-554E-4183-87C4-1AEEFEC801B7");     // CES
			public static readonly Guid ClearanceImportAir = new Guid("137CBBB6-2488-441A-A8BD-DD0B985891B7");     // CIA
			public static readonly Guid ClearanceImportRail = new Guid("D1085AC9-4950-47CE-9211-BBC575A46C35");    // CIL
			public static readonly Guid ClearanceImportRoad = new Guid("6B0DE052-1C4C-4B1A-8FE8-072E928D492B");    // CIR
			public static readonly Guid ClearanceImportSea = new Guid("A81A6208-8BBE-4669-B21E-BDF058677F00");     // CIS
			public static readonly Guid ClearancePost = new Guid("50D936A7-58DE-4D83-8284-614BCC17B118");          // CPP
			public static readonly Guid ClearanceOther = new Guid("3BC44454-A9C4-46A5-A5B1-C704C3605AE9");         // COT
			public static readonly Guid ClearanceImportOther = new Guid("0DBEC05B-8289-43BD-8AC9-0F32BC496AB2");   // CIT
			public static readonly Guid ClearanceExBond = new Guid("432D350D-037E-4620-AEC8-A422DCC2FC7D");        // CXB

			public static readonly Guid CfsPackAir = new Guid("5A04047D-6027-4E48-BB41-B6640A863ABB");             // DEA
			public static readonly Guid CfsPackRail = new Guid("01298D62-6BA9-49F2-985B-8F7279A34231");            // DEL
			public static readonly Guid CfsPackRoad = new Guid("CE45A9D7-2064-42FE-8BA8-EAEBE1563B9D");            // DER
			public static readonly Guid CfsPackSea = new Guid("0869AAB7-37BC-419C-A3D0-99A85511741C");             // DES

			public static readonly Guid CfsUnpackAir = new Guid("5CD0B66A-A038-4507-935E-8CD62CCF0D9A");           // DIA
			public static readonly Guid CfsUnpackRail = new Guid("66CA1C58-C63A-44A4-AB1E-B7067B435E79");          // DIL
			public static readonly Guid CfsUnpackRoad = new Guid("D29EAA21-0C14-4D60-9EDE-693FBAA84F25");          // DIR
			public static readonly Guid CfsUnpackSea = new Guid("602FB270-515D-4D04-847C-B4CB7D1F18CE");           // DIS

			public static readonly Guid TransportBookingDefaultDepartment = new Guid("DB20A0C6-1FE5-42C6-8226-0982043F9E06");           //TOT
		}

		public static class GroupPKs
		{
			public static readonly Guid Notification = new Guid("55896E13-12BE-4FD4-AC94-2956795A5BE2");
			public static Guid HostingSupport { get { return HostingSupportSingleton.Instance.HostingSupport; } }

			[Immutable]
#if DEBUG
			internal
#endif
			class HostingSupportSingleton
			{
				public static HostingSupportSingleton Instance { get { return lazy.Value; } }
				static readonly Lazy<HostingSupportSingleton> lazy = new Lazy<HostingSupportSingleton>(() => new HostingSupportSingleton());

				public Guid HostingSupport { get { return hostingSupport; } }
				public readonly Guid hostingSupport;

				HostingSupportSingleton()
				{
					hostingSupport = GetHostingSupport();
				}

				[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
				[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "CW1161:Do not use constant string literals - use resource strings instead.", Justification = "Database query")]
				internal Guid GetHostingSupport()
				{
					var result = Guid.Empty;

					if (EnvProxy.IsHostedWithCargowise)
					{
						using (var cmd = Db.Connection.Command("SELECT GG_PK FROM dbo.GlbGroup WHERE GG_Code = @code"))
						{
							cmd.AddParameterBasedOnDbColumn("@code", "SUP", GlbGroupSchema.GG_Code);
							var value = cmd.ExecuteScalar();

							if (value is Guid)
							{
								result = (Guid)value;
							}
						}
					}

					return result;
				}
			}
		}

		public static class Strings
		{
			public const string AccountType = "AccountType";

			public static MultilingualString ClosingText
			{
				get { return ResString.GetMultilingualString("80a465a5-31f3-48cb-bb78-1a27830ce328", "Closing Text"); }
			}

			public static MultilingualString ControlAccount
			{
				get { return ResString.GetMultilingualString("62e504c2-bb14-4eae-836d-4e8ca9a325bd", "Control Account"); }
			}

			public const string DefaultFreightChargeCode = "FRT";

			public static ResourceString DefaultRequestForMissingDocumentsClause
			{
				get { return ResString.GetMultilingualString("844603a7-1707-4451-b446-34920eb70ece", "We have not yet received documents for the shipment referenced herein. Please send the documents requested below, by E-mail attachment or Fax. If you have any problem that might delay the matter further, please contact the writer urgently. Otherwise, we look forward to receiving these documents as soon as possible. Without them, the completion of Customs formalities may not proceed and delivery will be delayed."); }
			}

			public static MultilingualString OpeningText
			{
				get { return ResString.GetMultilingualString("30e4b607-2f42-46fa-a100-7de964698895", "Opening Text"); }
			}
		}
	}
}

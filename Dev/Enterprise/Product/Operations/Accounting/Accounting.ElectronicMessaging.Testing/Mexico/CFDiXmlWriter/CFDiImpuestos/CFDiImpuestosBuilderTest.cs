using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Accounting.ElectronicMessaging.Mexico.Testing
{
	class CFDiImpuestosBuilderTest : TestCaseWithFactory
	{
		public void TestItUsedAsDependency()
		{
			AssertType<CFDiImpuestosBuilder>(new CFDiComprobanteBuilder().CFDiImpuestosBuilder_ExposedForTestOnly);
		}

		public void TestComprobanteImpuestos()
		{
			var builder = new CFDiImpuestosBuilder() as ICFDiImpuestosBuilder;
			var impuestos = builder.BuildComprobanteImpuestosInfo(ComprobanteConceptoObjectForTest);

			AssertEquals(1, impuestos.Retenciones.Length);
			AssertEquals(260m, impuestos.TotalImpuestosRetenidos);
			AssertComprobanteImpuestosRetencion("Unique Item", impuestos.Retenciones[0], 260m);
			Assert("Attribute must be True", impuestos.TotalImpuestosRetenidosSpecified);
		}

		void AssertComprobanteImpuestosRetencion(string message, ComprobanteImpuestosRetencion comprobanteConceptoImpuestosRetencion, ZDecimal expectedImporte)
		{
			AssertEquals(message + nameof(ComprobanteImpuestosRetencion.Impuesto), c_Impuesto.Item002, comprobanteConceptoImpuestosRetencion.Impuesto);
			AssertEquals(message + nameof(ComprobanteImpuestosRetencion.Importe), expectedImporte, comprobanteConceptoImpuestosRetencion.Importe);
		}

		public void TestComprobanteImpuestosTraslados()
		{
			var builder = new CFDiImpuestosBuilder() as ICFDiImpuestosBuilder;
			var impuestos = builder.BuildComprobanteImpuestosInfo(ComprobanteConceptoObjectForTestTraslados);

			AssertEquals(4, impuestos.Traslados.Length);
			AssertEquals(174m, impuestos.TotalImpuestosTrasladados);
			AssertEquals(true, impuestos.TotalImpuestosTrasladadosSpecified);
			AssertComprobanteImpuestos("Item 1", impuestos.Traslados[0], 0.16, 44m, c_TipoFactor.Tasa, 275m, true, true);
			AssertComprobanteImpuestos("Item 2", impuestos.Traslados[1], 0.0, 0m, c_TipoFactor.Exento, 153.23m, false, false);
			AssertComprobanteImpuestos("Item 3", impuestos.Traslados[2], 0.00, 0m, c_TipoFactor.Tasa, 562.5m, true, true);
			AssertComprobanteImpuestos("Item 4", impuestos.Traslados[3], 0.08, 130m, c_TipoFactor.Tasa, 1625m, true, true);
		}

		public void TestComprobanteImpuestos_With_Null_Traslados_And_Null_Retenciones()
		{
			var builder = new CFDiImpuestosBuilder() as ICFDiImpuestosBuilder;
			var comprobanteImpuestos = builder.BuildComprobanteImpuestosInfo(new ComprobanteConcepto[]
			{
				new ComprobanteConcepto()
				{
					Impuestos = new ComprobanteConceptoImpuestos()
					{
						Traslados = null,
						Retenciones = null
					}
				}
			});

			AssertNull(comprobanteImpuestos);
		}

		public void TestComprobanteImpuestos_With_Value_Traslados_And_Null_Retenciones()
		{
			var builder = new CFDiImpuestosBuilder() as ICFDiImpuestosBuilder;
			var comprobanteImpuestos = builder.BuildComprobanteImpuestosInfo(ComprobanteConceptoObjectForTestTraslados);

			AssertForTrasladosAndRetenciones(comprobanteImpuestos, false, true, 4, null, 174m);
		}

		public void TestComprobanteImpuestos_With_Null_Traslados_And_Value_Retenciones()
		{
			var builder = new CFDiImpuestosBuilder() as ICFDiImpuestosBuilder;
			var comprobanteImpuestos = builder.BuildComprobanteImpuestosInfo(ComprobanteConceptoObjectForTest);

			AssertForTrasladosAndRetenciones(comprobanteImpuestos, true, false, null, 1, 0m);
		}

		public void TestComprobanteImpuestos_With_Value_Traslados_And_Value_Retenciones()
		{
			var builder = new CFDiImpuestosBuilder() as ICFDiImpuestosBuilder;
			ComprobanteConcepto[] comprobanteConcepto = new ComprobanteConcepto[]
			{
				new ComprobanteConcepto()
				{
					Impuestos =
						new ComprobanteConceptoImpuestos()
						{
							Traslados = new[]
							{
								new ComprobanteConceptoImpuestosTraslado() { Importe = -10m, TasaOCuota = 0.16m },
							}
						}
				},
				new ComprobanteConcepto()
				{
					Impuestos =
						new ComprobanteConceptoImpuestos()
						{
							Retenciones = new[]
							{
								new ComprobanteConceptoImpuestosRetencion() { Importe = 130m },
							}
						}
				},
			};

			var comprobanteImpuestos = builder.BuildComprobanteImpuestosInfo(comprobanteConcepto);

			AssertForTrasladosAndRetenciones(comprobanteImpuestos, false, false, 1, 1, -10m);
		}

		public void TestComprobanteImpuestos_With_Value_Traslados_With_TotalImpuestosTrasladadosSpecified_false()
		{
			var builder = new CFDiImpuestosBuilder() as ICFDiImpuestosBuilder;
			ComprobanteConcepto[] comprobanteConcepto = new ComprobanteConcepto[]
			{
				new ComprobanteConcepto()
				{
					Impuestos =
						new ComprobanteConceptoImpuestos()
						{
							Traslados = new[]
							{
								new ComprobanteConceptoImpuestosTraslado() { Base = 25m,  Importe = 40m, TasaOCuota = 0.16m, TipoFactor = c_TipoFactor.Cuota },
								new ComprobanteConceptoImpuestosTraslado() { Base = 75m, TipoFactor = c_TipoFactor.Exento },
							}
						}
				},
			};

			var comprobanteImpuestos = builder.BuildComprobanteImpuestosInfo(comprobanteConcepto);

			AssertNotNull(comprobanteImpuestos.Traslados);
			AssertEquals(2, comprobanteImpuestos.Traslados.Length);
			AssertEquals(false, comprobanteImpuestos.TotalImpuestosTrasladadosSpecified);
			AssertEquals(40m, comprobanteImpuestos.TotalImpuestosTrasladados);
		}

		void AssertComprobanteImpuestos(string message, ComprobanteImpuestosTraslado comprobanteConceptoImpuestosTraslado, ZDecimal expectedTasaOcuota, ZDecimal expectedImporte, c_TipoFactor expectedTipoFactor, ZDecimal expectedBase, bool expectedImporteSpecified, bool expectedTasaOCuotaSpecified)
		{
			AssertEquals(message + nameof(ComprobanteImpuestosTraslado.Base), expectedBase, comprobanteConceptoImpuestosTraslado.Base);
			AssertEquals(message + nameof(ComprobanteImpuestosTraslado.Impuesto), c_Impuesto.Item002, comprobanteConceptoImpuestosTraslado.Impuesto);
			AssertEquals(message + nameof(ComprobanteImpuestosTraslado.TipoFactor), expectedTipoFactor, comprobanteConceptoImpuestosTraslado.TipoFactor);
			AssertEquals(message + nameof(ComprobanteImpuestosTraslado.TasaOCuota), expectedTasaOcuota, comprobanteConceptoImpuestosTraslado.TasaOCuota);
			AssertEquals(message + nameof(ComprobanteImpuestosTraslado.TasaOCuotaSpecified), expectedTasaOCuotaSpecified, comprobanteConceptoImpuestosTraslado.TasaOCuotaSpecified);
			AssertEquals(message + nameof(ComprobanteImpuestosTraslado.Importe), expectedImporte, comprobanteConceptoImpuestosTraslado.Importe);
			AssertEquals(message + nameof(ComprobanteImpuestosTraslado.ImporteSpecified), expectedImporteSpecified, comprobanteConceptoImpuestosTraslado.ImporteSpecified);
		}

		void AssertForTrasladosAndRetenciones(ComprobanteImpuestos comprobanteImpuestos, bool trasladosNull, bool retencionesNull, int? expectedTrasladosLength, int? expectedRetencionesLength, decimal expectedTotalImpuestosTrasladados)
		{
			AssertNotNull(comprobanteImpuestos);

			if (trasladosNull)
			{
				AssertNull(comprobanteImpuestos.Traslados);
				AssertEquals(false, comprobanteImpuestos.TotalImpuestosTrasladadosSpecified);
				AssertEquals(expectedTotalImpuestosTrasladados, comprobanteImpuestos.TotalImpuestosTrasladados);
			}
			else
			{
				AssertNotNull(comprobanteImpuestos.Traslados);
				AssertEquals(expectedTrasladosLength, comprobanteImpuestos.Traslados.Length);
				AssertEquals(true, comprobanteImpuestos.TotalImpuestosTrasladadosSpecified);
				AssertEquals(expectedTotalImpuestosTrasladados, comprobanteImpuestos.TotalImpuestosTrasladados);
			}

			if (retencionesNull)
			{
				AssertNull(comprobanteImpuestos.Retenciones);
				AssertEquals(false, comprobanteImpuestos.TotalImpuestosRetenidosSpecified);
				AssertEquals(true, comprobanteImpuestos.TotalImpuestosTrasladadosSpecified);
				AssertEquals(expectedTotalImpuestosTrasladados, comprobanteImpuestos.TotalImpuestosTrasladados);
			}
			else
			{
				AssertNotNull(comprobanteImpuestos.Retenciones);
				AssertEquals(expectedRetencionesLength, comprobanteImpuestos.Retenciones.Length);
				AssertEquals(true, comprobanteImpuestos.TotalImpuestosRetenidosSpecified);
				AssertEquals(expectedTotalImpuestosTrasladados, comprobanteImpuestos.TotalImpuestosTrasladados);
			}
		}

		#region Implementation

		ComprobanteConcepto[] ComprobanteConceptoObjectForTest => new ComprobanteConcepto[]
		{
			new ComprobanteConcepto()
			{
				Impuestos =
					new ComprobanteConceptoImpuestos()
					{
						Retenciones = new[]
						{
							new ComprobanteConceptoImpuestosRetencion { Importe = -10m },
							new ComprobanteConceptoImpuestosRetencion { Importe = 80m },
							new ComprobanteConceptoImpuestosRetencion { Importe = 50m },
						}
					}
			},
			new ComprobanteConcepto()
			{
				Impuestos =
					new ComprobanteConceptoImpuestos()
					{
						Retenciones = new[]
						{
							new ComprobanteConceptoImpuestosRetencion() { Importe = -40m },
							new ComprobanteConceptoImpuestosRetencion() { Importe = 50m },
						}
					}
			},
			new ComprobanteConcepto()
			{
				Impuestos =
					new ComprobanteConceptoImpuestos()
					{
						Retenciones = new[]
						{
							new ComprobanteConceptoImpuestosRetencion() { Importe = 130m },
						}
					}
			},
			new ComprobanteConcepto()
			{
				Impuestos = null
			},

			new ComprobanteConcepto()
			{
				Impuestos = new ComprobanteConceptoImpuestos()
				{
					Retenciones = null
				}
			},
		};

		ComprobanteConcepto[] ComprobanteConceptoObjectForTestTraslados => new ComprobanteConcepto[]
		{
				new ComprobanteConcepto()
				{
					Impuestos =
						new ComprobanteConceptoImpuestos()
						{
							Traslados = new[]
							{
								new ComprobanteConceptoImpuestosTraslado() { Base = -62.5m, Importe = -10m, TasaOCuota = 0.16m, TipoFactor = c_TipoFactor.Tasa },
							}
						}
				},
				new ComprobanteConcepto()
				{
					Impuestos =
						new ComprobanteConceptoImpuestos()
						{
							Traslados = new[]
							{
								new ComprobanteConceptoImpuestosTraslado() { Base = 125m, Importe = 20m, TasaOCuota = 0.16m, TipoFactor = c_TipoFactor.Tasa },
							}
						}
				},
				new ComprobanteConcepto()
				{
					Impuestos =
						new ComprobanteConceptoImpuestos()
						{
							Traslados = new[]
							{
								new ComprobanteConceptoImpuestosTraslado() { Base = 187.5m, Importe = 30m, TasaOCuota = 0.16m, TipoFactor = c_TipoFactor.Tasa },
							}
						}
				},
				new ComprobanteConcepto()
				{
					Impuestos =
						new ComprobanteConceptoImpuestos()
						{
							Traslados = new[]
							{
								new ComprobanteConceptoImpuestosTraslado() { Base = 25m,  Importe = 4m, TasaOCuota = 0.16m, TipoFactor = c_TipoFactor.Tasa },
								new ComprobanteConceptoImpuestosTraslado() { Base = 75m, TipoFactor = c_TipoFactor.Exento },
							}
						}
				},
				new ComprobanteConcepto()
				{
					Impuestos =
						new ComprobanteConceptoImpuestos()
						{
							Traslados = new[]
							{
								new ComprobanteConceptoImpuestosTraslado() { Base = 250m,  Importe = 0m, TasaOCuota = 0.00m, TipoFactor = c_TipoFactor.Tasa },
								new ComprobanteConceptoImpuestosTraslado() { Base = 312.5m, Importe = 0m, TasaOCuota = 0.00m, TipoFactor = c_TipoFactor.Tasa },
							}
						}
				},
				new ComprobanteConcepto()
				{
					Impuestos =
						new ComprobanteConceptoImpuestos()
						{
							Traslados = new[]
							{
								new ComprobanteConceptoImpuestosTraslado() { Base = 750m, Importe = 60m, TasaOCuota = 0.08m, TipoFactor = c_TipoFactor.Tasa },
							}
						}
				},
				new ComprobanteConcepto()
				{
					Impuestos =
						new ComprobanteConceptoImpuestos()
						{
							Traslados = new[]
							{
								new ComprobanteConceptoImpuestosTraslado() { Base = 875m, Importe = 70m, TasaOCuota = 0.08m, TipoFactor = c_TipoFactor.Tasa },
								null
							}
						}
				},
				new ComprobanteConcepto()
				{
					Impuestos =
						new ComprobanteConceptoImpuestos()
						{
							Traslados = new[]
							{
								new ComprobanteConceptoImpuestosTraslado() { Base = 78.23m,  TipoFactor = c_TipoFactor.Exento }
							}
						}
				},new ComprobanteConcepto()
				{
					Impuestos = null
				},
				new ComprobanteConcepto()
				{
					Impuestos = new ComprobanteConceptoImpuestos()
					{
						Traslados = null
					}
				},
		};

		#endregion

	}
}

# Data Schema Changelog EXAMPLE

## [v4] - 2024-10-16

### Added

- SpecificColumns
	- BizObjSchema.BO_RenamedAnotherNewColumn (Renamed from BizObjSchema.BO_AnotherNewColumn)

## [v3] - 2023-03-03

### Added

- SpecificColumns
	- BizObjSchema.BO_AnotherNewColumn
- NonBizObjColumns
	- ColumnInfo("BO_SomeNonBizObjColumn", "nvarchar(max)", isNullable: false)

## [v2] - 2023-02-02

### Added

- SpecificColumns
	- BizObjSchema.BO_NewColumn
- LegacyColumns
	- BizObjSchema.BO_LegacyColumn

### Changed

- SpecificColumns
	- BizObjSchema.BO_Category from varchar(3) to varchar(4)

### Removed

- SpecificColumns
	- BizObjSchema.BO_Category

## [v1] - 2020-01-01

### Initial schema

- SpecificColumns
    - BizObjSchema.PK
    - BizObjSchema.BO_Category
    - BizObjSchema.BO_SystemCreateTimeUtc

import { Pipe, PipeTransform } from '@angular/core';

@Pipe({ name: 'nameFilter' })
export class FilterPipe implements PipeTransform {
  transform(items: any[], text: string): any[] {
    if (!items) {
      return [];
    }
    if (!text) {
      return items;
    }
    text = text.toLowerCase();

    return items.filter((value) => value.name.toLowerCase().indexOf(text) !== -1);
  }
}
